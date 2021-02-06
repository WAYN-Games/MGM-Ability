using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(AbilityCostsCheckerSystemGroup))]
    public abstract class AbilityCostCheckerSystem<COST, RESOURCE, COST_HANDLER> : SystemBase
      where COST : struct, IAbilityCost
      where RESOURCE : struct, IComponentData
      where COST_HANDLER : struct, ICostHandler<COST, RESOURCE>
    {

        /// <summary>
        /// The base query to select entity that are eligible to this system.
        /// </summary>
        private EntityQuery _query;

        /// <summary>
        /// A map of all effects' unmutable data for the EFFECT type.
        /// </summary>
        private NativeMultiHashMap<uint, COST> _costMap;

        protected override void OnCreate()
        {
            base.OnCreate();
            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadOnly<AbilityBufferElement>(),
                        ComponentType.ReadOnly<RESOURCE>()
                }
            });
            World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnCostUpdate += UpdateCostCache;
            Enabled = false;
        }

        protected sealed override void OnUpdate()
        {
            Dependency = new CostHandlerJob()
            {
                AbilityBufferChunk = GetBufferTypeHandle<AbilityBufferElement>(false),
                ResourceComponent = GetComponentTypeHandle<RESOURCE>(true),
                CostMap = _costMap,
                CostHandler = GetCostHandler()

            }.ScheduleParallel(_query, Dependency);

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_costMap.IsCreated) _costMap.Dispose();
        }

        /// <summary>
        /// Job in charge of the shared logic (targetting, ability activity,..).
        /// This job will call the WriteContextualizedEffect method of the CTX_WRITER when the efect has to be triggered.
        /// </summary>
        [BurstCompile]
        public struct CostHandlerJob : IJobChunk
        {
            public COST_HANDLER CostHandler;
            public BufferTypeHandle<AbilityBufferElement> AbilityBufferChunk;
            [ReadOnly] public NativeMultiHashMap<uint, COST> CostMap;
            [ReadOnly] public ComponentTypeHandle<RESOURCE> ResourceComponent;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<AbilityBufferElement> abilityBufffers = chunk.GetBufferAccessor(AbilityBufferChunk);
                NativeArray<RESOURCE> resourceComponent = chunk.GetNativeArray(ResourceComponent);

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    RESOURCE resource = resourceComponent[entityIndex];
                    NativeArray<AbilityBufferElement> AbilityBufferArray = abilityBufffers[entityIndex].AsNativeArray();
                    for (int abilityIndex = 0; abilityIndex < AbilityBufferArray.Length; ++abilityIndex)
                    {
                        AbilityBufferElement ability = AbilityBufferArray[abilityIndex];
                        bool temp = true;
                        NativeMultiHashMap<uint, COST>.Enumerator enumerator = CostMap.GetValuesForKey(AbilityBufferArray[abilityIndex].Guid);
                        while (enumerator.MoveNext())
                        {
                            COST cost = enumerator.Current;
                            temp &= CostHandler.HasEnougthResourceLeft(cost, in resource);
                        }
                        ability.HasEnougthRessource &= temp;
                        AbilityBufferArray[abilityIndex] = ability;
                    }
                }

            }
        }

        protected virtual COST_HANDLER GetCostHandler()
        {
            return default;
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

        private static NativeMultiHashMap<uint, COST> BuildEffectMapCache(MultiHashMap<Type, CostData> effectMap)
        {
            NativeMultiHashMap<uint, COST> map = new NativeMultiHashMap<uint, COST>(effectMap.Count(typeof(COST)), Allocator.Persistent);
            foreach (CostData costData in effectMap[typeof(COST)])
            {
                map.Add(costData.Guid, (COST)costData.cost);
            }
            return map;
        }
    }

}
