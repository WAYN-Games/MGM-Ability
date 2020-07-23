
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


namespace WaynGroup.Mgm.Ability
{
    public interface IEffectContext<EFFECT> where EFFECT : struct, IEffect
    {
        Entity Target { get; set; }
        EFFECT Effect { get; set; }
    }

    [UpdateInGroup(typeof(AbilityConsumerSystemGroup))]
    public abstract class AbilityEffectConsumerSystem<EFFECT, EFFECT_CTX> : SystemBase where EFFECT : struct, IEffect
        where EFFECT_CTX : struct, IEffectContext<EFFECT>
    {

        /// <summary>
        ///  The stream to Read/Write the contextualized effect. 
        /// </summary>
        private NativeStream _effectStream;

        /// <summary>
        /// A map o effect per targeted entity to improve consumer job performance.
        /// </summary>
        protected NativeMultiHashMap<Entity, EFFECT_CTX> _effects;

        private int _forEachCount;

        /// <summary>
        /// The trigger job handle to make sure we finished trigerring all necessary effect before consuming the effects.
        /// </summary>
        private JobHandle TriggerJobHandle;

        protected override void OnCreate()
        {
            base.OnCreate();

            // Allocate the map only on create to avoid allocating every frame.
            _effects = new NativeMultiHashMap<Entity, EFFECT_CTX>(0, Allocator.Persistent);
        }


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

        /// <summary>
        /// This job reads all the effects to apply and dsipatche them into a map by targeted entity.
        /// This ensures better performance overall in consuming the effect.
        /// </summary>
        [BurstCompile]
        struct RemapEffects : IJobParallelFor
        {
            [ReadOnly] public NativeStream.Reader EffectReader;
            public NativeMultiHashMap<Entity, EFFECT_CTX>.ParallelWriter EffectsWriter;
            public void Execute(int index)
            {
                EffectReader.BeginForEachIndex(index);
                int rangeItemCount = EffectReader.RemainingItemCount;
                for (int j = 0; j < rangeItemCount; j++)
                {

                    EFFECT_CTX effect = EffectReader.Read<EFFECT_CTX>();
                    EffectsWriter.Add(effect.Target, effect);
                }

                EffectReader.EndForEachIndex();
            }
        }

        /// <summary>
        /// Clear the effect map and allocate additional capacity if needed.
        /// </summary>
        [BurstCompile]
        struct SetupEffectMap : IJob
        {
            [ReadOnly] public NativeStream.Reader EffectReader;
            public NativeMultiHashMap<Entity, EFFECT_CTX> Effects;
            public void Execute()
            {
                Effects.Clear();
                if (Effects.Capacity < EffectReader.Count())
                {
                    Effects.Capacity = EffectReader.Count();
                }
            }
        }

        protected sealed override void OnUpdate()
        {
            Dependency = JobHandle.CombineDependencies(Dependency, TriggerJobHandle);

            // If the producer did not actually write anything to the stream, the native stream will not be flaged as created.
            // In that case we don't need to do anything.
            // Not doing this checks actually result in a non authrorized access to the memory and crashes Unity.
            if (!_effectStream.IsCreated) return;
            NativeStream.Reader effectReader = GetEffectReader();
            SetupEffectMap AllocateJob = new SetupEffectMap()
            {
                EffectReader = effectReader,
                Effects = _effects
            };
            Dependency = AllocateJob.Schedule(Dependency);


            NativeMultiHashMap<Entity, EFFECT_CTX>.ParallelWriter effectsWriter = _effects.AsParallelWriter();
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

        public NativeStream.Reader GetEffectReader()
        {
            return _effectStream.AsReader();
        }
    }
}
