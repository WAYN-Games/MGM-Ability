using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(AbilityConsumerSystemGroup))]
    public abstract partial class AbilityEffectConsumerSystem<EFFECT, EFFECT_CTX> : SystemBase where EFFECT : struct, IEffect
        where EFFECT_CTX : struct, IEffectContext
    {
        #region Protected Fields

        /// <summary>
        /// A map o effect per targeted entity to improve consumer job performance.
        /// </summary>
        protected NativeMultiHashMap<Entity, ContextualizedEffect> _effects;

        #endregion Protected Fields

        #region Private Fields

        /// <summary>
        ///  The stream to Read/Write the contextualized effect.
        /// </summary>
        private List<NativeStream> _effectStreams;

        private List<int> _forEachCounts;

        /// <summary>
        /// The trigger job handle to make sure we finished trigerring all necessary effect before consuming the effects.
        /// </summary>
        private JobHandle TriggerJobHandle;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Setup the dependecy between the trigger job and the consumer job to make sure we finished trigerring all necessary effect before consuming the effects.
        /// </summary>
        /// <param name="triggerJobHandle">The trigger job JobHandle.</param>
        public void RegisterTriggerDependency(JobHandle triggerJobHandle)
        {
            TriggerJobHandle = triggerJobHandle;
        }

        /// <summary>
        /// Get a NativeStream.Writer to write the effects to consume.
        /// </summary>
        /// <param name="foreachCount">The number of chunk of thread that writes to the NativeStream</param>
        /// <returns></returns>
        public NativeStream.Writer CreateConsumerWriter(int foreachCount)
        {
            var _effectStream = new NativeStream(foreachCount, Allocator.TempJob);
            _effectStreams.Add(_effectStream);
            _forEachCounts.Add(foreachCount);
            //Debug.Log($"New Writer ({_effectStreams.Count})");
            return _effectStream.AsWriter();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnCreate()
        {
            base.OnCreate();
            _effectStreams = new List<NativeStream>();
            _forEachCounts = new List<int>();
            // Allocate the map only on create to avoid allocating every frame.
            _effects = new NativeMultiHashMap<Entity, ContextualizedEffect>(0, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var stream in _effectStreams)
            {
                if (stream.IsCreated)
                {
                    stream.Dispose(Dependency);
                }
            }
            if (_effects.IsCreated)
            {
                _effects.Dispose(Dependency);
            }
        }

        /// <summary>
        /// Delegate the effect consumption logic to the derived class.
        /// </summary>
        protected abstract void Consume();

        protected override sealed void OnUpdate()
        {
            Dependency = JobHandle.CombineDependencies(Dependency, TriggerJobHandle);

            #region Make sure there is something to do

            // If the producer did not actually write anything to the stream, the native stream will
            // not be flaged as created. In that case we don't need to do anything and we can remove
            // the stream from the list of stream to process Not doing the IsCreated check actually
            // result in a non authrorized access to the memory and crashes Unity.
            for (int i = _effectStreams.Count - 1; i >= 0; i--)
            {
                if (!_effectStreams[i].IsCreated)
                {
                    _effectStreams.RemoveAt(i);
                    _forEachCounts.RemoveAt(i);
                }
            }

            // if there are no stream left to process, do nothing
            if (_effectStreams.Count == 0) return;


            #endregion Make sure there is something to do

            #region Setup effect map and ensure it's capacity

            NativeArray<int> effectCounts = new NativeArray<int>(_effectStreams.Count, Allocator.TempJob);
            var inputDeps = Dependency;
            var outDeps = new NativeArray<JobHandle>(_effectStreams.Count + 1, Allocator.TempJob);
            outDeps[0] = Dependency;
            for (int i = 0; i < _effectStreams.Count; i++)
            {
                outDeps[i + 1] = new CountEffectJob()
                {
                    streamReader = _effectStreams[i].AsReader(),
                    index = i,
                    count = effectCounts
                }.Schedule(inputDeps);
            }
            Dependency = JobHandle.CombineDependencies(outDeps);
            outDeps.Dispose(Dependency);
            EnsureCapcityJob AllocateJob = new EnsureCapcityJob()
            {
                EffectCounts = effectCounts,
                Effects = _effects
            };
            Dependency = AllocateJob.Schedule(Dependency);
            effectCounts.Dispose(Dependency);

            #endregion Setup effect map and ensure it's capacity

            #region Remap Effect to their targeted entity

            NativeMultiHashMap<Entity, ContextualizedEffect>.ParallelWriter effectsWriter = _effects.AsParallelWriter();

            for (int i = 0; i < _effectStreams.Count; i++)
            {
                Dependency = new RemapEffects()
                {
                    EffectReader = _effectStreams[i].AsReader(),
                    EffectsWriter = _effects.AsParallelWriter()
                }.Schedule(_forEachCounts[i], 1, Dependency);
            }

            #endregion Remap Effect to their targeted entity

            // Call the effect consumption logic defined in the derived class.
            Consume();
            for (int i = 0; i < _effectStreams.Count; i++)
            {
                _effectStreams[i].Dispose(Dependency);
            }
            _effectStreams.Clear();
        }

        #endregion Protected Methods

        #region Protected Structs

        protected struct ContextualizedEffect
        {
            #region Public Fields

            public EFFECT Effect;
            public EFFECT_CTX Context;

            #endregion Public Fields
        }

        #endregion Protected Structs

        #region Private Structs

        [BurstCompatible]
        private struct CountEffectJob : IJob
        {
            #region Public Fields

            [NativeDisableParallelForRestriction]
            public NativeArray<int> count;

            [ReadOnly] public int index;
            [ReadOnly] public NativeStream.Reader streamReader;

            #endregion Public Fields

            #region Public Methods

            public void Execute()
            {
                count[index] = streamReader.Count();
            }

            #endregion Public Methods
        }

        /// <summary>
        /// This job reads all the effects to apply and dsipatche them into a map by targeted entity.
        /// This ensures better performance overall in consuming the effect.
        /// </summary>
        [BurstCompile]
        private struct RemapEffects : IJobParallelFor
        {
            #region Public Fields

            [ReadOnly] public NativeStream.Reader EffectReader;
            public NativeMultiHashMap<Entity, ContextualizedEffect>.ParallelWriter EffectsWriter;

            #endregion Public Fields

            #region Public Methods

            public void Execute(int index)
            {
                EffectReader.BeginForEachIndex(index);
                while (EffectReader.RemainingItemCount > 0)
                {
                    EFFECT effect = EffectReader.Read<EFFECT>();
                    EFFECT_CTX context = EffectReader.Read<EFFECT_CTX>();
                    int targetCount = EffectReader.Read<int>();
                    for (int i = 0; i < targetCount; ++i)
                    {
                        EffectsWriter.Add(EffectReader.Read<Entity>(), new ContextualizedEffect() { Effect = effect, Context = context });
                    }
                }
                EffectReader.EndForEachIndex();
            }

            #endregion Public Methods
        }

        /// <summary>
        /// Clear the effect map and allocate additional capacity if needed.
        /// </summary>
        [BurstCompile]
        private struct EnsureCapcityJob : IJob
        {
            #region Public Fields

            [ReadOnly] public NativeArray<int> EffectCounts;
            public NativeMultiHashMap<Entity, ContextualizedEffect> Effects;

            #endregion Public Fields

            #region Public Methods

            public void Execute()
            {
                var count = 0;
                for (int i = 0; i < EffectCounts.Length; i++)
                {
                    count += EffectCounts[i];
                }

                Debug.Log($"{count} Effect in total.");
                Effects.Clear();
                if (Effects.Capacity < count)
                {
                    Effects.Capacity = count;
                }
            }

            #endregion Public Methods
        }

        #endregion Private Structs
    }
}