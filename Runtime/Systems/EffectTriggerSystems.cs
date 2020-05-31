using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

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



        protected override void OnUpdate()
        {
            Dependency = new TriggerJob() // Entities.ForEach don't support generic
            {
                effectBufferChunk = GetArchetypeChunkBufferType<BUFFER>(true),
                skillBufferChunk = GetArchetypeChunkBufferType<SkillBuffer>(true),
                targetChunk = GetArchetypeChunkComponentType<Target>(true),
                ConsumerWriter = ConusmerSystem.GetConsumerWriter(Query.CalculateChunkCount())
            }.ScheduleParallel(Query, Dependency);
            ConusmerSystem.RegisterProducerDependency(Dependency);
        }
    }

}
