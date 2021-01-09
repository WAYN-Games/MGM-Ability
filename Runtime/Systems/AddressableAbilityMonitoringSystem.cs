using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Entities;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class AddressableAbilityCatalogSystem : SystemBase
    {

        public delegate void OnEffectUpdateDelegate(MultiMap<Type, EffectData> effectMap);
        public delegate void OnCostUpdateDelegate(MultiMap<Type, CostData> costMap);
        public delegate void OnAbilityUpdateDelegate(List<ScriptableAbility> abilityCatalogue);

        public OnEffectUpdateDelegate OnEffectUpdate;
        public OnCostUpdateDelegate OnCostUpdate;
        public OnAbilityUpdateDelegate OnAbilityUpdate;

        protected override void OnCreate()
        {
            base.OnCreate();
            OnAbilityUpdate += BuildEffectCatalogueAsync;
            OnAbilityUpdate += BuildCostCatalogueAsync;
            LoadAbilityCatalogueAsync();
            Enabled = false;
        }

        protected override void OnUpdate()
        {

        }

        #region Self Documenting Encapsulations


        private void LoadAbilityCatalogueAsync()
        {

            Debug.Log("Start catalogue synch");

            AsyncOperationHandle<IList<ScriptableAbility>> handle = Addressables.LoadAssetsAsync<ScriptableAbility>(new AssetLabelReference() { labelString = "Ability" }, null, false);

            SendAbilityCatalogueUpdateOnComplete(handle);

        }

        private void SendAbilityCatalogueUpdateOnComplete(AsyncOperationHandle<IList<ScriptableAbility>> handle)
        {
            handle.Completed += objects =>
            {

                List<ScriptableAbility> _abilities = new List<ScriptableAbility>();
                _abilities.AddRange(objects.Result);

                OnAbilityUpdate.Invoke(_abilities);
            };
        }

        private void BuildEffectCatalogueAsync(List<ScriptableAbility> _abilities)
        {

            Debug.Log("Update effect");
            Task task = new Task(
              () =>
              {
                  MultiMap<Type, EffectData> _effectMap = new MultiMap<Type, EffectData>();
                  for (int i = 0; i < _abilities.Count; ++i)
                  {

                      foreach (IEffect effect in _abilities[i].Effects)
                      {

                          _effectMap.Add(effect.GetType(), new EffectData()
                          {
                              guid = new Guid(_abilities[i].Id),
                              effect = effect
                          });
                      }
                  }
                  OnEffectUpdate.Invoke(_effectMap);
              });
            task.Start();
        }

        private void BuildCostCatalogueAsync(List<ScriptableAbility> _abilities)
        {

            Debug.Log("Update cost");
            Task task = new Task(
              () =>
              {
                  MultiMap<Type, CostData> _costMap = new MultiMap<Type, CostData>();
                  for (int i = 0; i < _abilities.Count; ++i)
                  {
                      Debug.Log($"Updating cost for {_abilities[i].Name}");
                      foreach (IAbilityCost cost in _abilities[i].Costs)
                      {

                          _costMap.Add(cost.GetType(), new CostData()
                          {
                              guid = new Guid(_abilities[i].Id),
                              cost = cost
                          });
                      }
                      Debug.Log($"Updated cost for {_abilities[i].Name}");
                  }
                  Debug.Log($"Invoke OnCostUpdate");
                  OnCostUpdate.Invoke(_costMap);
              });
            task.Start();
        }
        #endregion
    }
}
