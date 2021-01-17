using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Entities;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class AddressableAbilityCatalogSystem : SystemBase
    {

        public delegate void OnEffectUpdateDelegate(MultiMap<Type, EffectData> effectMap);
        public delegate void OnCostUpdateDelegate(MultiMap<Type, CostData> costMap);
        public delegate void OnAbilityUpdateDelegate(Dictionary<uint, ScriptableAbility> abilityCatalogue);

        public OnEffectUpdateDelegate OnEffectUpdate;
        public OnCostUpdateDelegate OnCostUpdate;
        public OnAbilityUpdateDelegate OnAbilityUpdate;

        public Dictionary<uint, ScriptableAbility> AbilityCatalog;

        protected override void OnCreate()
        {
            base.OnCreate();
            AbilityCatalog = new Dictionary<uint, ScriptableAbility>();
            OnAbilityUpdate += BuildEffectCatalogueAsync;
            OnAbilityUpdate += BuildCostCatalogueAsync;
            LoadAbilityCatalogueAsync();
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            // Nothing, it's an initialisation system.
        }

        #region Self Documenting Encapsulations


        private void LoadAbilityCatalogueAsync()
        {

            AsyncOperationHandle<IList<ScriptableAbility>> handle = Addressables.LoadAssetsAsync<ScriptableAbility>(new AssetLabelReference() { labelString = AbilityHelper.ADDRESSABLE_ABILITY_LABEL }, null, false);

            SendAbilityCatalogueUpdateOnComplete(handle);

        }

        private void SendAbilityCatalogueUpdateOnComplete(AsyncOperationHandle<IList<ScriptableAbility>> handle)
        {
            handle.Completed += objects =>
            {
                if (objects.Result == null) return;
                foreach (ScriptableAbility ability in objects.Result)
                {
                    AbilityCatalog.Add(ability.Id, ability);
                }
                OnAbilityUpdate.Invoke(AbilityCatalog);
            };
        }

        private void BuildEffectCatalogueAsync(Dictionary<uint, ScriptableAbility> _abilities)
        {
            Task task = new Task(
              () =>
              {
                  MultiMap<Type, EffectData> _effectMap = new MultiMap<Type, EffectData>();

                  foreach (KeyValuePair<uint, ScriptableAbility> keyValuePair in _abilities)
                  {
                      ScriptableAbility ability = keyValuePair.Value;
                      foreach (IEffect effect in ability.Effects)
                      {

                          _effectMap.Add(effect.GetType(), new EffectData()
                          {
                              Guid = ability.Id,
                              effect = effect
                          });
                      }
                  }
                  OnEffectUpdate.Invoke(_effectMap);
              });
            task.Start();
        }

        private void BuildCostCatalogueAsync(Dictionary<uint, ScriptableAbility> _abilities)
        {
            Task task = new Task(
              () =>
              {
                  MultiMap<Type, CostData> _costMap = new MultiMap<Type, CostData>();
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
        #endregion
    }
}
