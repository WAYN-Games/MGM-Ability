using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(AbilityCostsConsumerSystemGroup))]
    public abstract class AbilityCostConsumerSystem<COST, RESOURCE, COST_HANDLER> : SystemBase
        where COST : struct, IAbilityCost
        where RESOURCE : struct, IComponentData
        where COST_HANDLER : struct, ICostHandler<COST, RESOURCE>
    {
        #region Private Fields

        /// <summary>
        /// The base query to select entity that are eligible to this system.
        /// </summary>
        private EntityQuery _query;

        /// <summary>
        /// A map of all effects' unmutable data for the EFFECT type.
        /// </summary>
        private NativeMultiHashMap<uint, COST> _costMap;

        #endregion Private Fields

        #region Protected Methods

        protected override void OnCreate()
        {
            base.OnCreate();
            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadWrite<RESOURCE>(),
                        ComponentType.ReadOnly<AbilityInput>()
                }
            });
            World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnCostUpdate += UpdateCostCache;
            Enabled = false;
        }

        protected override sealed void OnUpdate()
        {
            Dependency = new CostHandlerJob()
            {
                AbilityInputComponent = GetComponentTypeHandle<AbilityInput>(true),
                ResourceComponent = GetComponentTypeHandle<RESOURCE>(false),
                CostMap = _costMap,
                CostHandler = GetCostHandler()
            }.ScheduleParallel(_query, Dependency);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_costMap.IsCreated) _costMap.Dispose();
        }

        protected virtual COST_HANDLER GetCostHandler()
        {
            return default;
        }

        #endregion Protected Methods

        #region Private Methods

        private static NativeMultiHashMap<uint, COST> BuildEffectMapCache(MultiHashMap<Type, CostData> effectMap)
        {
            NativeMultiHashMap<uint, COST> map = new NativeMultiHashMap<uint, COST>(effectMap.Count(typeof(COST)), Allocator.Persistent);
            foreach (CostData costData in effectMap[typeof(COST)])
            {
                map.Add(costData.Guid, (COST)costData.cost);
            }
            return map;
        }

        private void UpdateCostCache(MultiHashMap<Type, CostData> costMap)
        {
            NativeMultiHashMap<uint, COST> map = BuildEffectMapCache(costMap);
            RefreshEffectMapChache(map);
            Enabled = true;
        }

        private void RefreshEffectMapChache(NativeMultiHashMap<uint, COST> map)
        {
            if (_costMap.IsCreated) _costMap.Dispose();
            _costMap = map;
        }

        #endregion Private Methods

        #region Public Structs

        /// <summary>
        /// Job in charge of the shared logic (targetting, ability activity,..).
        /// This job will call the WriteContextualizedEffect method of the CTX_WRITER when the efect has to be triggered.
        /// </summary>
        [BurstCompile]
        public struct CostHandlerJob : IJobChunk
        {
            #region Public Fields

            [ReadOnly] public ComponentTypeHandle<AbilityInput> AbilityInputComponent;
            [ReadOnly] public NativeMultiHashMap<uint, COST> CostMap;
            public COST_HANDLER CostHandler;
            public ComponentTypeHandle<RESOURCE> ResourceComponent;

            #endregion Public Fields

            #region Public Methods

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<RESOURCE> resourceComponent = chunk.GetNativeArray(ResourceComponent);
                NativeArray<AbilityInput> abilityInputComponent = chunk.GetNativeArray(AbilityInputComponent);

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    AbilityInput abilityInput = abilityInputComponent[entityIndex];
                    if (!abilityInput.IsApplicable()) continue;

                    RESOURCE resource = resourceComponent[entityIndex];
                    NativeMultiHashMap<uint, COST>.Enumerator enumerator = CostMap.GetValuesForKey(abilityInput.AbilityId);
                    while (enumerator.MoveNext())
                    {
                        COST cost = enumerator.Current;
                        CostHandler.ConsumeCost(cost, ref resource);
                        resourceComponent[entityIndex] = resource;
                    }
                }
            }

            #endregion Public Methods
        }

        #endregion Public Structs
    }
}