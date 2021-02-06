using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;

using UnityEngine.AddressableAssets;



namespace WaynGroup.Mgm.Ability
{

    public interface ICacheComponent : IComponentData, IDisposable
    {
    }

    public struct AbilityTimingsCache : ICacheComponent
    {
        public BlobAssetReference<BlobMultiHashMap<uint, AbilityTimings>> Cache { get; set; }

        public void Dispose()
        {
            if (Cache.IsCreated) Cache.Dispose();
        }
    }
    public struct RangeCache : ICacheComponent
    {
        public BlobAssetReference<BlobMultiHashMap<uint, Range>> Cache { get; set; }

        public void Dispose()
        {
            if (Cache.IsCreated) Cache.Dispose();
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class AddressableAbilityCatalogSystem : SystemBase
    {

        public delegate void OnEffectUpdateDelegate(MultiHashMap<Type, EffectData> effectMap);
        public delegate void OnCostUpdateDelegate(MultiHashMap<Type, CostData> costMap);
        public delegate void OnAbilityUpdateDelegate(Dictionary<uint, ScriptableAbility> abilityCatalogue);

        public OnEffectUpdateDelegate OnEffectUpdate;
        public OnCostUpdateDelegate OnCostUpdate;
        public OnAbilityUpdateDelegate OnAbilityUpdate;

        public Dictionary<uint, ScriptableAbility> AbilityCatalog;

        private struct AbilityCacheEntityTag : IComponentData { }

        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = false;

            AbilityCatalog = new Dictionary<uint, ScriptableAbility>();
            OnAbilityUpdate += BuildEffectCatalogueAsync;
            OnAbilityUpdate += BuildCostCatalogueAsync;
            OnAbilityUpdate += RefreshRangeCacheAsync;
            OnAbilityUpdate += RefreshTimmingsCacheAsync;
            LoadAbilityCatalogueAsync();

        }

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


        #region Timmings Cache
        private void RefreshTimmingsCacheAsync(Dictionary<uint, ScriptableAbility> abilityCatalogue)
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
        #endregion

        #region Range Cache
        private void RefreshRangeCacheAsync(Dictionary<uint, ScriptableAbility> abilityCatalogue)
        {


            using (BlobBuilder bb = new BlobBuilder(Allocator.Temp))
            {
                var mapBuilder = new BlobHashMapBuilder<uint, Range>(bb);
                foreach (KeyValuePair<uint, ScriptableAbility> ability in abilityCatalogue)
                {
                    mapBuilder.Add(ability.Key, ability.Value.Range);
                }
                var newCache = mapBuilder.CreateBlobAssetReference(Allocator.Persistent);
                RefreshCache(new RangeCache() { Cache = newCache });
            }


        }
        #endregion

        protected override void OnUpdate()
        {
            // Nothing, it's an initialisation system.
        }

        #region Self Documenting Encapsulations


        private void LoadAbilityCatalogueAsync()
        {

            Addressables.LoadAssetsAsync<ScriptableAbility>(new AssetLabelReference()
            {
                labelString = AbilityHelper.ADDRESSABLE_ABILITY_LABEL
            }, null, false).Completed += objects =>
            {
                if (objects.Result == null) return;
                foreach (ScriptableAbility ability in objects.Result)
                {
                    AbilityCatalog.Add(ability.Id, ability);
                }
                OnAbilityUpdate.Invoke(AbilityCatalog);
            }; ;

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            World.EntityManager.DestroyEntity(GetEntityQuery(typeof(AbilityCacheEntityTag)));
        }
        #endregion
    }
}
