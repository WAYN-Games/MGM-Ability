using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace WaynGroup.Mgm.Skill
{
    [UpdateBefore(typeof(SkillDeactivationSystem))]
    public abstract class SkillRessourcePoolCheckSystem<BUFFER, EFFECT, POOL> : SystemBase
    where EFFECT : struct, IPoolModifierEffect
    where BUFFER : struct, IEffectBufferElement<EFFECT>
        where POOL : struct, IPoolComponentData
    {
        private EntityQuery Query;

        protected override void OnCreate()
        {
            base.OnCreate();


            Query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadOnly<SkillBuffer>(),
                        ComponentType.ReadOnly<POOL>(),
                        ComponentType.ReadOnly<BUFFER>()
                }
            });
        }

        [BurstCompile]
        struct PoolCheckJob : IJobChunk
        {
            public ArchetypeChunkBufferType<SkillBuffer> skillBufferChunk;
            [ReadOnly] public ArchetypeChunkComponentType<POOL> poolChunk;
            [ReadOnly] public ArchetypeChunkBufferType<BUFFER> effectBufferChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<SkillBuffer> skillBufffers = chunk.GetBufferAccessor(skillBufferChunk);
                NativeArray<POOL> poolChunkArray = chunk.GetNativeArray(poolChunk);
                BufferAccessor<BUFFER> effectBuffers = chunk.GetBufferAccessor(effectBufferChunk);


                for (int entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
                {
                    NativeArray<SkillBuffer> SkillBufferArray = skillBufffers[entityIndex].AsNativeArray();
                    NativeArray<BUFFER> effectBufferArray = effectBuffers[entityIndex].AsNativeArray();
                    POOL pool = poolChunkArray[entityIndex];
                    for (int i = 0; i < SkillBufferArray.Length; i++)
                    {
                        Skill Skill = SkillBufferArray[i];
                        if (!Skill.ShouldApplyEffects()) continue;
                        for (int e = 0; e < effectBufferArray.Length; e++)
                        {
                            BUFFER EffectBuffer = effectBufferArray[i];
                            if (EffectBuffer.SkillIndex != Skill.Index) continue;

                            EffectBuffer.Effect.HasEnougthResources(pool.Value);


                        }

                    }


                }
            }
        }



        protected override void OnUpdate()
        {
            Dependency = new PoolCheckJob() // Entities.ForEach don't support generic
            {
                effectBufferChunk = GetArchetypeChunkBufferType<BUFFER>(false),
                skillBufferChunk = GetArchetypeChunkBufferType<SkillBuffer>(true),
                poolChunk = GetArchetypeChunkComponentType<POOL>(true)
            }.ScheduleParallel(Query, Dependency);
        }
    }

}
