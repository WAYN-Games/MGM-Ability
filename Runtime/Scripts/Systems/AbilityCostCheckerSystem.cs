using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(AbilityCostsCheckerSystemGroup))]
    public abstract partial class AbilityCostCheckerSystem<COST, RESOURCE, COST_HANDLER> : SystemBase
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
        private NativeParallelMultiHashMap<uint, COST> _costMap;

        #endregion Private Fields

        #region Protected Methods

        protected override void OnCreate()
        {
            base.OnCreate();
            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadOnly<RESOURCE>(),
                        ComponentType.ReadWrite<AbilityInput>()
                }
            });
            _costMap = new NativeParallelMultiHashMap<uint, COST>(0, Allocator.Persistent);
            World.GetOrCreateSystem<AddressableAbilityCatalogSystem>().OnCostUpdate += UpdateCostCache;
            Enabled = false;
        }

        protected override sealed void OnUpdate()
        {
            Dependency = new CostHandlerJob()
            {
                ResourceComponent = GetComponentTypeHandle<RESOURCE>(true),
                AbilityInputComponent = GetComponentTypeHandle<AbilityInput>(),
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

        private void UpdateCostCache(MultiHashMap<Type, CostData> costMap)
        {
            Enabled = false;
            _costMap.Clear();
            _costMap.Capacity = costMap.Count(typeof(COST));
            foreach (CostData costData in costMap[typeof(COST)])
            {
                _costMap.Add(costData.Guid, (COST)costData.cost);
            }
            Enabled = true;
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

            public COST_HANDLER CostHandler;
            public ComponentTypeHandle<AbilityInput> AbilityInputComponent;
            [ReadOnly] public NativeParallelMultiHashMap<uint, COST> CostMap;
            [ReadOnly] public ComponentTypeHandle<RESOURCE> ResourceComponent;

            #endregion Public Fields

            #region Public Methods

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<RESOURCE> resourceComponent = chunk.GetNativeArray(ResourceComponent);
                NativeArray<AbilityInput> abilityInputComponent = chunk.GetNativeArray(AbilityInputComponent);
                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    AbilityInput input = abilityInputComponent[entityIndex];
                    if (input.IsEnabled())
                    {
                        RESOURCE resource = resourceComponent[entityIndex];
                        bool temp = true;
                        NativeParallelMultiHashMap<uint, COST>.Enumerator enumerator = CostMap.GetValuesForKey(input.AbilityId);
                        while (enumerator.MoveNext())
                        {
                            COST cost = enumerator.Current;
                            temp &= CostHandler.HasEnougthResourceLeft(cost, in resource);
                        }
                        if (!temp)
                        {
                            input.AddRestriction(4);
                        }
                        abilityInputComponent[entityIndex] = input;
                    }
                }
            }

            #endregion Public Methods
        }

        #endregion Public Structs
    }
}