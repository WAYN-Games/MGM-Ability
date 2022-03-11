﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(AbilityConsumerSystemGroup))]
    public abstract class AbilityEffectConsumerSystem<EFFECT, EFFECT_CTX> : SystemBase where EFFECT : struct, IEffect
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
        private NativeStream _effectStream;

        private int _forEachCount;

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
            _effectStream = new NativeStream(foreachCount, Allocator.TempJob);
            _forEachCount = foreachCount;
            return _effectStream.AsWriter();
        }

        public NativeStream.Reader GetEffectReader()
        {
            return _effectStream.AsReader();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void OnCreate()
        {
            base.OnCreate();

            // Allocate the map only on create to avoid allocating every frame.
            _effects = new NativeMultiHashMap<Entity, ContextualizedEffect>(0, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_effectStream.IsCreated)
            {
                _effectStream.Dispose(Dependency);
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

            // If the producer did not actually write anything to the stream, the native stream will not be flaged as created.
            // In that case we don't need to do anything.
            // Not doing the IsCreated check actually result in a non authrorized access to the memory and crashes Unity.
            if (!_effectStream.IsCreated) return;
            NativeStream.Reader effectReader = GetEffectReader();
            SetupEffectMap AllocateJob = new SetupEffectMap()
            {
                EffectReader = effectReader,
                Effects = _effects
            };
            Dependency = AllocateJob.Schedule(Dependency);

            NativeMultiHashMap<Entity, ContextualizedEffect>.ParallelWriter effectsWriter = _effects.AsParallelWriter();
            RemapEffects RemapEffectsJob = new RemapEffects()
            {
                EffectReader = effectReader,
                EffectsWriter = _effects.AsParallelWriter()
            };
            Dependency = RemapEffectsJob.Schedule(_forEachCount, 1, Dependency);

            // Call the effect consumption logic defined in hte derived class.
            Consume();

            Dependency = _effectStream.Dispose(Dependency);
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
        private struct SetupEffectMap : IJob
        {
            #region Public Fields

            [ReadOnly] public NativeStream.Reader EffectReader;
            public NativeMultiHashMap<Entity, ContextualizedEffect> Effects;

            #endregion Public Fields

            #region Public Methods

            public void Execute()
            {
                Effects.Clear();
                if (Effects.Capacity < EffectReader.Count())
                {
                    Effects.Capacity = EffectReader.Count();
                }
            }

            #endregion Public Methods
        }

        #endregion Private Structs
    }
}