using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{


    [UpdateBefore(typeof(SkillDeactivationSystem))]
    public abstract class EffectTriggerSystem<BUFFER, EFFECT, CONSUMER> : SystemBase
        where EFFECT : struct, IEffect
        where CONSUMER : EffectConsumerSystem<EFFECT>
        where BUFFER : struct, IEffectBufferElement<EFFECT>
    {
        private EffectConsumerSystem<EFFECT> ConusmerSystem;
        private EntityQuery Query;

        protected override void OnCreate()
        {
            base.OnCreate();
            ConusmerSystem = World.GetOrCreateSystem<CONSUMER>();
            Query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadOnly<SkillBuffer>(),
                        ComponentType.ReadOnly<Target>(),
                        ComponentType.ReadOnly<BUFFER>()
                }
            });
        }


        public interface IEffectContextWriter
        {
            void WriteContextualizedEffect(ArchetypeChunk chunk, int entityIndex, ref NativeStream.Writer ConsumerWriter, EFFECT Effect);
        }

        [BurstCompile]
        public struct TriggerJob<T> : IJobChunk
                where T : struct, IEffectContextWriter
        {
            public T EffectContextWriter;
            [ReadOnly] public ArchetypeChunkBufferType<SkillBuffer> skillBufferChunk;
            [ReadOnly] public ArchetypeChunkBufferType<BUFFER> effectBufferChunk;
            public NativeStream.Writer ConsumerWriter;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<SkillBuffer> skillBufffers = chunk.GetBufferAccessor(skillBufferChunk);
                BufferAccessor<BUFFER> effectBuffers = chunk.GetBufferAccessor(effectBufferChunk);

                ConsumerWriter.BeginForEachIndex(chunkIndex);
                for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
                {


                    NativeArray<SkillBuffer> SkillBufferArray = skillBufffers[entityIndex].AsNativeArray();
                    NativeArray<BUFFER> effectBufferArray = effectBuffers[entityIndex].AsNativeArray();
                    for (int i = 0; i < SkillBufferArray.Length; i++)
                    {

                        Skill Skill = SkillBufferArray[i];
                        if (!Skill.ShouldApplyEffects()) continue;
                        for (int e = 0; e < effectBufferArray.Length; e++)
                        {
                            BUFFER EffectBuffer = effectBufferArray[i];
                            if (EffectBuffer.SkillIndex != Skill.Index) continue;

                            EffectContextWriter.WriteContextualizedEffect(chunk, entityIndex, ref ConsumerWriter, EffectBuffer.Effect);

                        }

                    }


                }
                ConsumerWriter.EndForEachIndex();

            }
        }

        [BurstCompile]
        public struct TargetEffectWriter : IEffectContextWriter
        {
            [ReadOnly] public ArchetypeChunkComponentType<Target> targetChunk;

            public void WriteContextualizedEffect(ArchetypeChunk chunk, int entityIndex, ref NativeStream.Writer ConsumerWriter, EFFECT Effect)
            {
                NativeArray<Target> targets = chunk.GetNativeArray(targetChunk);
                ConsumerWriter.Write(new ContextualizedEffect<EFFECT>() { Target = targets[entityIndex].Value, Effect = Effect });

            }
        }
        /*
        [BurstCompile]
        struct TriggerJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkBufferType<SkillBuffer> skillBufferChunk;
            [ReadOnly] public ArchetypeChunkComponentType<Target> targetChunk;
            [ReadOnly] public ArchetypeChunkBufferType<BUFFER> effectBufferChunk;
            public NativeStream.Writer ConsumerWriter;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<SkillBuffer> skillBufffers = chunk.GetBufferAccessor(skillBufferChunk);
                NativeArray<Target> targets = chunk.GetNativeArray(targetChunk);
                BufferAccessor<BUFFER> effectBuffers = chunk.GetBufferAccessor(effectBufferChunk);

                ConsumerWriter.BeginForEachIndex(chunkIndex);
                for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
                {
                    NativeArray<SkillBuffer> SkillBufferArray = skillBufffers[entityIndex].AsNativeArray();
                    NativeArray<BUFFER> effectBufferArray = effectBuffers[entityIndex].AsNativeArray();
                    for (int i = 0; i < SkillBufferArray.Length; i++)
                    {
                        Skill Skill = SkillBufferArray[i];
                        if (!Skill.ShouldApplyEffects()) continue;
                        for (int e = 0; e < effectBufferArray.Length; e++)
                        {
                            BUFFER EffectBuffer = effectBufferArray[i];
                            if (EffectBuffer.SkillIndex != Skill.Index) continue;


                            // Actual logic, the rest is boiler plate code...
                            // That is where I would had to the context the data from the emiter that are needed by the effect, like power or position, ...
                            ConsumerWriter.Write(new ContextualizedEffect<EFFECT>() { Target = targets[entityIndex].Value, Effect = EffectBuffer.Effect });

                        }

                    }


                }
                ConsumerWriter.EndForEachIndex();
            }
        }

   */

        protected override void OnUpdate()
        {
            Dependency = new TriggerJob<TargetEffectWriter>() // Entities.ForEach don't support generic
            {
                effectBufferChunk = GetArchetypeChunkBufferType<BUFFER>(true),
                skillBufferChunk = GetArchetypeChunkBufferType<SkillBuffer>(true),
                ConsumerWriter = ConusmerSystem.GetConsumerWriter(Query.CalculateChunkCount()),
                EffectContextWriter = new TargetEffectWriter()
                {
                    targetChunk = GetArchetypeChunkComponentType<Target>(true)
                }
            }.ScheduleParallel(Query, Dependency);
            ConusmerSystem.RegisterProducerDependency(Dependency);
        }
    }

}
