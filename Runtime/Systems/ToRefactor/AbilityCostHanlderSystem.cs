using System;

using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{


    [UpdateInGroup(typeof(AbilityCostsSystemGroup))]
    [UpdateAfter(typeof(AbilityCostCheckInitializationSystem))]
    public abstract class AbilityCostHanlderSystem<COST, RESOURCE, COST_HANDLER> : SystemBase
        where COST : struct, IAbilityCost
        where RESOURCE : struct, IComponentData
        where COST_HANDLER : struct, ICostHandler<COST, RESOURCE>

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
                        ComponentType.ReadOnly<AbilityBufferElement>(),
                        ComponentType.ReadWrite<RESOURCE>()
                }
            });
            World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnCostUpdate += UpdateCostCache;
            Enabled = false;
        }


        private void UpdateCostCache(MultiMap<Type, CostData> costMap)
        {
            UnityEngine.Debug.Log("CostCache building");
            NativeMultiHashMap<Guid, COST> map = BuildEffectMapCache(costMap);
            RefreshEffectMapChache(map);
            Enabled = true;
            UnityEngine.Debug.Log("CostCache built");
        }

        private void RefreshEffectMapChache(NativeMultiHashMap<Guid, COST> map)
        {
            if (_costMap.IsCreated) _costMap.Dispose();
            _costMap = map;
        }

        /// <summary>
        /// A map of all effects' unmutable data for the EFFECT type.
        /// </summary>
        private NativeMultiHashMap<Guid, COST> _costMap;

        private static NativeMultiHashMap<Guid, COST> BuildEffectMapCache(MultiMap<Type, CostData> effectMap)
        {
            NativeMultiHashMap<Guid, COST> map = new NativeMultiHashMap<Guid, COST>(effectMap.Count(typeof(COST)), Allocator.Persistent);
            foreach (CostData costData in effectMap[typeof(COST)])
            {
                map.Add(costData.guid, (COST)costData.cost);
            }
            return map;
        }

        /// <summary>
        /// Job in charge of the shared logic (targetting, ability activity,..).
        /// This job will call the WriteContextualizedEffect method of the CTX_WRITER when the efect has to be triggered.
        /// </summary>
        private struct CostConsumerJob : IJobChunk
        {
            public COST_HANDLER CostHandler;
            public BufferTypeHandle<AbilityBufferElement> AbilityBufferChunk;
            [ReadOnly] public NativeMultiHashMap<Guid, COST> CostMap;
            public ComponentTypeHandle<RESOURCE> ResourceComponent;

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
                        NativeMultiHashMap<Guid, COST>.Enumerator enumerator = CostMap.GetValuesForKey(AbilityBufferArray[abilityIndex].Guid);
                        while (enumerator.MoveNext())
                        {
                            COST cost = enumerator.Current;
                            temp &= CostHandler.HasEnougthResourceLeft(cost, in resource);
                        }
                        ability.HasEnougthRessource &= temp;
                        AbilityBufferArray[abilityIndex] = ability;


                        if (ability.AbilityState != AbilityState.Active || !ability.HasEnougthRessource) continue;
                        UnityEngine.Debug.Log($"Ability {ability.Guid} is active. And HasEnougthRessource {ability.HasEnougthRessource}");

                        enumerator.Reset();
                        while (enumerator.MoveNext())
                        {
                            COST cost = enumerator.Current;

                            CostHandler.ConsumeCost(cost, ref resource);
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
                AbilityBufferChunk = GetBufferTypeHandle<AbilityBufferElement>(false),
                ResourceComponent = GetComponentTypeHandle<RESOURCE>(false),
                CostMap = _costMap,
                CostHandler = GetCostConsumer()

            }.ScheduleParallel(_query, Dependency);

        }

        protected abstract COST_HANDLER GetCostConsumer();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_costMap.IsCreated) _costMap.Dispose();
        }
    }

}
