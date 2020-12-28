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
        public delegate void OnAbilityUpdateDelegate(List<ScriptableAbility> abilityCatalogue);

        public OnEffectUpdateDelegate OnEffectUpdate;
        public OnAbilityUpdateDelegate OnAbilityUpdate;

        protected override void OnCreate()
        {
            base.OnCreate();
            OnAbilityUpdate += BuildEffectCatalogueAsync;
            LoadAbilityCatalogueAsync();
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            // Nothing to do
        }

        #region Self Documenting Encapsulations

        private AsyncOperationHandle<IList<ScriptableAbility>> LoadAbilityCatalogueAsync()
        {
            AsyncOperationHandle<IList<ScriptableAbility>> handle = Addressables.LoadAssetsAsync<ScriptableAbility>(new AssetLabelReference() { labelString = "Ability" }, null, false);
            SendAbilityCatalogueUpdateOnComplete(handle);
            return handle;
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
        #endregion
    }
}
