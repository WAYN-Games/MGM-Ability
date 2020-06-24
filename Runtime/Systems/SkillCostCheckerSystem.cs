
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{

    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    public abstract class AbilityCostCheckerSystem<COST_BUFFER, COST, COST_CHECKER, RESOURCE> : SystemBase
        where COST : struct, IAbilityCost
        where COST_BUFFER : struct, IAbilityCostBufferElement<COST>
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
                        ComponentType.ReadOnly<AbilityBuffer>(),
                        ComponentType.ReadOnly<COST_BUFFER>(),
                        ComponentType.ReadOnly<RESOURCE>()
                }
            });

        }

        [BurstCompile]
        private struct CostCheckerJob : IJobChunk
        {
            public ArchetypeChunkBufferType<AbilityBuffer> AbilityBufferChunk;
            [ReadOnly] public ArchetypeChunkBufferType<COST_BUFFER> CostBufferChunk;
            [ReadOnly] public ArchetypeChunkComponentType<RESOURCE> ResourceChunk;
            [ReadOnly] public COST_CHECKER CostChecker;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AbilityBuffer> abilityBufffers = chunk.GetBufferAccessor(AbilityBufferChunk);
                BufferAccessor<COST_BUFFER> costBuffers = chunk.GetBufferAccessor(CostBufferChunk);
                NativeArray<RESOURCE> resources = chunk.GetNativeArray(ResourceChunk);

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    NativeArray<AbilityBuffer> AbilityBufferArray = abilityBufffers[entityIndex].AsNativeArray();
                    NativeArray<COST_BUFFER> costBufferArray = costBuffers[entityIndex].AsNativeArray();
                    RESOURCE resource = resources[entityIndex];
                    for (int abilityIndex = 0; abilityIndex < AbilityBufferArray.Length; ++abilityIndex)
                    {

                        Ability Ability = AbilityBufferArray[abilityIndex];
                        bool temp = true;
                        for (int costIndex = 0; costIndex < costBufferArray.Length; ++costIndex)
                        {
                            COST_BUFFER CostBuffer = costBufferArray[costIndex];
                            if (CostBuffer.AbilityIndex != abilityIndex) continue;
                            temp &= CostChecker.HasEnougthResourceLeft(CostBuffer.Cost, in resource);
                        }
                        Ability.HasEnougthRessource &= temp;
                        AbilityBufferArray[abilityIndex] = Ability;
                    }
                }
            }
        }

        protected abstract COST_CHECKER GetCostChecker();

        protected override void OnUpdate()
        {
            Dependency = new CostCheckerJob()
            {
                AbilityBufferChunk = GetArchetypeChunkBufferType<AbilityBuffer>(false),
                CostBufferChunk = GetArchetypeChunkBufferType<COST_BUFFER>(true),
                ResourceChunk = GetArchetypeChunkComponentType<RESOURCE>(true),
                CostChecker = GetCostChecker()
            }.ScheduleParallel(_query, Dependency);
        }
    }


}
