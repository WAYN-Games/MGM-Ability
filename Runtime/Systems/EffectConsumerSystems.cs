
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace WaynGroup.Mgm.Skill
{






    public abstract class EffectConsumerSystem<EFFECT> : SystemBase where EFFECT : struct, IEffect
    {
        // FIXME should be set private, temporary set public for test purposes.
        public NativeStream EffectStream;

        private NativeMultiHashMap<Entity, EFFECT> Effects;

        private JobHandle ProducerJobHandle;


        public void RegisterProducerDependency(JobHandle jh)
        {
            ProducerJobHandle = jh;
        }

        public NativeStream.Writer GetConsumerWriter(int foreachCount)
        {
            EffectStream = new NativeStream(foreachCount, Allocator.TempJob);
            return EffectStream.AsWriter();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (EffectStream.IsCreated)
            {
                EffectStream.Dispose(Dependency);
            }
            if (Effects.IsCreated)
            {
                Effects.Dispose(Dependency);
            }
        }

        protected abstract void Consume();

        protected NativeMultiHashMap<Entity, EFFECT> GetEffects()
        {
            return Effects;
        }

        [BurstCompile]
        struct RemapEffects : IJobParallelFor
        {
            [ReadOnly] public NativeStream.Reader effectReader;
            public NativeMultiHashMap<Entity, EFFECT>.ParallelWriter effectsWriter;
            public void Execute(int index)
            {
                effectReader.BeginForEachIndex(index);
                int rangeItemCount = effectReader.RemainingItemCount;
                for (int j = 0; j < rangeItemCount; j++)
                {

                    ContextualizedEffect<EFFECT> effect = effectReader.Read<ContextualizedEffect<EFFECT>>();
                    effectsWriter.Add(effect.Target, effect.Effect);
                }

                effectReader.EndForEachIndex();
            }
        }

        [BurstCompile]
        struct Allocate : IJob
        {
            [ReadOnly] public NativeStream.Reader effectReader;
            public NativeMultiHashMap<Entity, EFFECT> effects;
            public void Execute()
            {
                effects.Clear();
                if (effects.Capacity < effectReader.ComputeItemCount())
                {
                    effects.Capacity = effectReader.ComputeItemCount();
                }
            }
        }

        protected sealed override void OnUpdate()
        {

            Dependency = JobHandle.CombineDependencies(Dependency, ProducerJobHandle);
            if (!EffectStream.IsCreated) return;
            NativeStream.Reader effectReader = EffectStream.AsReader();


            Allocate AllocateJob = new Allocate()
            {
                effectReader = effectReader,
                effects = Effects
            };

            NativeMultiHashMap<Entity, EFFECT>.ParallelWriter effectsWriter = Effects.AsParallelWriter();

            RemapEffects RemapEffectsJob = new RemapEffects()
            {
                effectReader = effectReader,
                effectsWriter = Effects.AsParallelWriter()
            };

            Dependency = AllocateJob.Schedule(Dependency);
            Dependency = RemapEffectsJob.Schedule(effectReader.ForEachCount, 1, Dependency);

            Consume();


            Dependency = EffectStream.Dispose(Dependency);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            Effects = new NativeMultiHashMap<Entity, EFFECT>(0, Allocator.Persistent);
        }
    }
}
