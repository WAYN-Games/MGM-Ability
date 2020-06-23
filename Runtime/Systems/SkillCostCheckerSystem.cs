
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Skill.Demo
{
    [UpdateInGroup(typeof(SkillUpdateSystemGroup))]
    public abstract class SkillCostCheckerSystem<COST_BUFFER, COST, COST_CHECKER, RESOURCE> : SystemBase
        where COST : struct, ISkillCost
        where COST_BUFFER : struct, ISkillCostBufferElement<COST>
        where RESOURCE : struct, IComponentData
        where COST_CHECKER : struct, ICostChecker<RESOURCE, COST>
    {

        private EntityQuery _query;

        protected override void OnCreate()
        {
            base.OnCreate();

            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadOnly<COST_BUFFER>(),
                        ComponentType.ReadOnly<RESOURCE>()
                }
            });

        }

        [BurstCompile]
        private struct CostCheckerJob : IJobChunk
        {
            public ArchetypeChunkBufferType<SkillBuffer> SkillBufferChunk;
            [ReadOnly] public ArchetypeChunkBufferType<COST_BUFFER> CostBufferChunk;
            [ReadOnly] public ArchetypeChunkComponentType<RESOURCE> ResourceChunk;
            [ReadOnly] public COST_CHECKER CostChecker;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<SkillBuffer> skillBufffers = chunk.GetBufferAccessor(SkillBufferChunk);
                BufferAccessor<COST_BUFFER> costBuffers = chunk.GetBufferAccessor(CostBufferChunk);
                NativeArray<RESOURCE> resources = chunk.GetNativeArray(ResourceChunk);

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    NativeArray<SkillBuffer> SkillBufferArray = skillBufffers[entityIndex].AsNativeArray();
                    NativeArray<COST_BUFFER> costBufferArray = costBuffers[entityIndex].AsNativeArray();
                    RESOURCE resource = resources[entityIndex];
                    for (int skillIndex = 0; skillIndex < SkillBufferArray.Length; ++skillIndex)
                    {

                        Skill Skill = SkillBufferArray[skillIndex];
                        bool temp = true;
                        for (int costIndex = 0; costIndex < costBufferArray.Length; ++costIndex)
                        {
                            COST_BUFFER CostBuffer = costBufferArray[costIndex];
                            if (CostBuffer.SkillIndex != skillIndex) continue;
                            temp &= CostChecker.HasEnougthResourceLeft(CostBuffer.Cost, in resource);
                        }
                        Skill.HasEnougthRessource &= temp;
                        SkillBufferArray[skillIndex] = Skill;
                    }
                }
            }
        }

        protected abstract COST_CHECKER GetCostChecker();

        protected override void OnUpdate()
        {
            Dependency = new CostCheckerJob()
            {
                SkillBufferChunk = GetArchetypeChunkBufferType<SkillBuffer>(false),
                CostBufferChunk = GetArchetypeChunkBufferType<COST_BUFFER>(true),
                ResourceChunk = GetArchetypeChunkComponentType<RESOURCE>(true),
                CostChecker = GetCostChecker()
            }.ScheduleParallel(_query, Dependency);
        }
    }


}
