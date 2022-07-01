using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class AddressableAbilityCatalogSystem : SystemBase
    {
        #region Public Fields

        public OnEffectUpdateDelegate OnEffectUpdate;

        public OnCostUpdateDelegate OnCostUpdate;

        public OnAbilityUpdateDelegate OnAbilityUpdate;

        public Dictionary<uint, ScriptableAbility> AbilityCatalog;

        #endregion Public Fields

        #region Public Delegates

        public delegate void OnEffectUpdateDelegate(MultiHashMap<Type, EffectData> effectMap);

        public delegate void OnCostUpdateDelegate(MultiHashMap<Type, CostData> costMap);

        public delegate void OnAbilityUpdateDelegate(Dictionary<uint, ScriptableAbility> abilityCatalogue);

        #endregion Public Delegates

        #region Protected Methods

        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = false;

            AbilityCatalog = new Dictionary<uint, ScriptableAbility>();
            OnAbilityUpdate += BuildEffectCatalogueAsync;
            OnAbilityUpdate += BuildCostCatalogueAsync;
            OnAbilityUpdate += RefreshRangeCache;
            OnAbilityUpdate += RefreshTimmingsCache;
            StartLoadingAbilities();
        }

        protected override void OnUpdate()
        {
            // Nothing, it's an initialisation system.
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            World.EntityManager.DestroyEntity(GetEntityQuery(typeof(AbilityCacheEntityTag)));
        }

        #endregion Protected Methods

        #region Private Methods

        private void RefreshCache<T>(T newCache) where T : struct, ICacheComponent
        {
            Entity cacheEntity;
            if (!TryGetSingletonEntity<AbilityCacheEntityTag>(out cacheEntity))
            {
                cacheEntity = World.EntityManager.CreateEntity();
                World.EntityManager.AddComponent(cacheEntity, typeof(AbilityCacheEntityTag));
            }
            if (!World.EntityManager.HasComponent<T>(cacheEntity))
            {
                World.EntityManager.AddComponent(cacheEntity, typeof(T));
            }

            if (TryGetSingleton(out T cache))
            {
                cache.Dispose();
            }

            SetSingleton(newCache);
        }

        private void RefreshTimmingsCache(Dictionary<uint, ScriptableAbility> abilityCatalogue)
        {
            using (BlobBuilder bb = new BlobBuilder(Allocator.Temp))
            {
                var mapBuilder = new BlobHashMapBuilder<uint, AbilityTimings>(bb);
                foreach (KeyValuePair<uint, ScriptableAbility> ability in abilityCatalogue)
                {
                    mapBuilder.Add(ability.Key, ability.Value.Timings);
                }
                var newCache = mapBuilder.CreateBlobAssetReference(Allocator.Persistent);
                RefreshCache(new AbilityTimingsCache() { Cache = newCache });
            }
        }

        private void RefreshRangeCache(Dictionary<uint, ScriptableAbility> abilityCatalogue)
        {
            using (BlobBuilder bb = new BlobBuilder(Allocator.Temp))
            {
                var mapBuilder = new BlobHashMapBuilder<uint, Range>(bb);
                foreach (KeyValuePair<uint, ScriptableAbility> ability in abilityCatalogue)
                {
                    mapBuilder.Add(ability.Key, new Range() { Min = math.pow(ability.Value.Range.Min, 2), Max = math.pow(ability.Value.Range.Max, 2) });
                }
                var newCache = mapBuilder.CreateBlobAssetReference(Allocator.Persistent);
                RefreshCache(new SquaredRangeCache() { Cache = newCache });
            }
        }

        private void LoadAbilityCatalogueAsync()
        {
            //Debug.Log($"Loading Addressable Abilities...");
            Addressables.LoadAssetsAsync<ScriptableAbility>(new AssetLabelReference()
            {
                labelString = AbilityHelper.ADDRESSABLE_ABILITY_LABEL
            }, null, false).Completed += objects =>
            {
                //Debug.Log($"Loading Addressable Abilities complete");
                if (objects.Result == null) return;
                //Debug.Log($"Found {objects.Result.Count} Addressable Abilities");
                foreach (ScriptableAbility ability in objects.Result)
                {
                    //Debug.Log($"Adding {ability.Id} to catalogue");
                    AbilityCatalog.Add(ability.Id, ability);
                }
                OnAbilityUpdate.Invoke(AbilityCatalog);

                Addressables.Release(objects);
            };
        }

        private void StartLoadingAbilities()
        {
            //Returns any IResourceLocations that are mapped to the key "AssetKey"
            Addressables.LoadResourceLocationsAsync(new AssetLabelReference()
            {
                labelString = AbilityHelper.ADDRESSABLE_ABILITY_LABEL
            }, typeof(ScriptableAbility)).Completed += objects =>
           {
               if (objects.Result.Count > 0)
               {
                   LoadAbilityCatalogueAsync();
               }
               Addressables.Release(objects);
           };
        }

        private void BuildEffectCatalogueAsync(Dictionary<uint, ScriptableAbility> _abilities)
        {
            Task task = new Task(
              () =>
              {
                  MultiHashMap<Type, EffectData> effectMap = new MultiHashMap<Type, EffectData>();

                  foreach (KeyValuePair<uint, ScriptableAbility> keyValuePair in _abilities)
                  {
                      ScriptableAbility ability = keyValuePair.Value;
                      foreach (IEffect effect in ability.Effects)
                      {
                          effectMap.Add(effect.GetType(), new EffectData()
                          {
                              Guid = ability.Id,
                              effect = effect
                          });
                      }
                  }
                  OnEffectUpdate.Invoke(effectMap);
              });
            task.Start();
        }

        private void BuildCostCatalogueAsync(Dictionary<uint, ScriptableAbility> _abilities)
        {
            Task task = new Task(
              () =>
              {
                  MultiHashMap<Type, CostData> _costMap = new MultiHashMap<Type, CostData>();
                  foreach (KeyValuePair<uint, ScriptableAbility> keyValuePair in _abilities)
                  {
                      ScriptableAbility ability = keyValuePair.Value;
                      foreach (IAbilityCost cost in ability.Costs)
                      {
                          _costMap.Add(cost.GetType(), new CostData()
                          {
                              Guid = ability.Id,
                              cost = cost
                          });
                      }
                  }
                  OnCostUpdate.Invoke(_costMap);
              });
            task.Start();
        }

        #endregion Private Methods

        #region Private Structs

        private struct AbilityCacheEntityTag : IComponentData
        { }

        #endregion Private Structs
    }
}