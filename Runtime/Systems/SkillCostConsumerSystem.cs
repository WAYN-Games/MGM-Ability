using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{

    public interface ICostConsumer<COST, RESOURCE> where RESOURCE : struct, IComponentData
    {
        void ConsumeCost(COST cost, ref RESOURCE resource);
    }

    [UpdateInGroup(typeof(SkillTriggerSystemGroup))]
    public abstract class SkillCostConsumerSystem<COST, COST_BUFFER, RESOURCE, COST_CONSUMER> : SystemBase where COST : struct, ISkillCost
        where COST_BUFFER : struct, ISkillCostBufferElement<COST>
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
                        ComponentType.ReadOnly<SkillBuffer>(),
                        ComponentType.ReadOnly<COST_BUFFER>(),
                        ComponentType.ReadWrite<RESOURCE>()
                }
            });

        }

        /// <summary>
        /// Job in charge of the shared logic (targetting, skill activity,..).
        /// This job will call the WriteContextualizedEffect method of the CTX_WRITER when the efect has to be triggered.
        /// </summary>
        [BurstCompile]
        private struct CostConsumerJob : IJobChunk
        {
            public COST_CONSUMER CostConsumer;
            [ReadOnly] public ArchetypeChunkBufferType<SkillBuffer> SkillBufferChunk;
            [ReadOnly] public ArchetypeChunkBufferType<COST_BUFFER> CostBufferChunk;
            public ArchetypeChunkComponentType<RESOURCE> ResourceComponent;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<SkillBuffer> skillBufffers = chunk.GetBufferAccessor(SkillBufferChunk);
                BufferAccessor<COST_BUFFER> costBuffers = chunk.GetBufferAccessor(CostBufferChunk);
                NativeArray<RESOURCE> resourceComponent = chunk.GetNativeArray(ResourceComponent);

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    NativeArray<SkillBuffer> SkillBufferArray = skillBufffers[entityIndex].AsNativeArray();
                    NativeArray<COST_BUFFER> costBufferArray = costBuffers[entityIndex].AsNativeArray();
                    for (int skillIndex = 0; skillIndex < SkillBufferArray.Length; ++skillIndex)
                    {
                        Skill Skill = SkillBufferArray[skillIndex];


                        if (Skill.State != SkillState.Active) continue;
                        for (int costIndex = 0; costIndex < costBufferArray.Length; costIndex++)
                        {
                            COST_BUFFER costBuffer = costBufferArray[costIndex];
                            if (costBuffer.SkillIndex != skillIndex) continue;
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
                CostBufferChunk = GetArchetypeChunkBufferType<COST_BUFFER>(true),
                SkillBufferChunk = GetArchetypeChunkBufferType<SkillBuffer>(true),
                ResourceComponent = GetArchetypeChunkComponentType<RESOURCE>(false),
                CostConsumer = GetCostConsumer()

            }.ScheduleParallel(_query, Dependency);

        }

        protected abstract COST_CONSUMER GetCostConsumer();
    }

}
