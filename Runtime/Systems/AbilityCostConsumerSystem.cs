using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{

    public interface ICostConsumer<COST, RESOURCE> where RESOURCE : struct, IComponentData
    {
        void ConsumeCost(COST cost, ref RESOURCE resource);
    }

    [UpdateInGroup(typeof(AbilityTriggerSystemGroup))]
    public abstract class AbilityCostConsumerSystem<COST, COST_BUFFER, RESOURCE, COST_CONSUMER> : SystemBase where COST : struct, IAbilityCost
        where COST_BUFFER : struct, IAbilityCostBufferElement<COST>
        where RESOURCE : struct, IComponentData
        where COST_CONSUMER : struct, ICostConsumer<COST, RESOURCE>

    {


        /// <summary>
        /// The base query to select entity that are eligible to this system.
        /// </summary>
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
                        ComponentType.ReadWrite<RESOURCE>()
                }
            });

        }

        /// <summary>
        /// Job in charge of the shared logic (targetting, ability activity,..).
        /// This job will call the WriteContextualizedEffect method of the CTX_WRITER when the efect has to be triggered.
        /// </summary>
        [BurstCompile]
        private struct CostConsumerJob : IJobChunk
        {
            public COST_CONSUMER CostConsumer;
            [ReadOnly] public BufferTypeHandle<AbilityBuffer> AbilityBufferChunk;
            [ReadOnly] public BufferTypeHandle<COST_BUFFER> CostBufferChunk;
            public ComponentTypeHandle<RESOURCE> ResourceComponent;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AbilityBuffer> abilityBufffers = chunk.GetBufferAccessor(AbilityBufferChunk);
                BufferAccessor<COST_BUFFER> costBuffers = chunk.GetBufferAccessor(CostBufferChunk);
                NativeArray<RESOURCE> resourceComponent = chunk.GetNativeArray(ResourceComponent);

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    NativeArray<AbilityBuffer> AbilityBufferArray = abilityBufffers[entityIndex].AsNativeArray();
                    NativeArray<COST_BUFFER> costBufferArray = costBuffers[entityIndex].AsNativeArray();
                    for (int abilityIndex = 0; abilityIndex < AbilityBufferArray.Length; ++abilityIndex)
                    {
                        Ability Ability = AbilityBufferArray[abilityIndex];


                        if (Ability.State != AbilityState.Active) continue;
                        for (int costIndex = 0; costIndex < costBufferArray.Length; costIndex++)
                        {
                            COST_BUFFER costBuffer = costBufferArray[costIndex];
                            if (costBuffer.AbilityIndex != abilityIndex) continue;
                            RESOURCE resource = resourceComponent[entityIndex];
                            CostConsumer.ConsumeCost(costBuffer.Cost, ref resource);
                            resourceComponent[entityIndex] = resource;
                        }
                    }
                }

            }
        }

        protected sealed override void OnUpdate()
        {
            Dependency = new CostConsumerJob()
            {
                CostBufferChunk = GetBufferTypeHandle<COST_BUFFER>(true),
                AbilityBufferChunk = GetBufferTypeHandle<AbilityBuffer>(true),
                ResourceComponent = GetComponentTypeHandle<RESOURCE>(false),
                CostConsumer = GetCostConsumer()

            }.ScheduleParallel(_query, Dependency);

        }

        protected abstract COST_CONSUMER GetCostConsumer();
    }

}
