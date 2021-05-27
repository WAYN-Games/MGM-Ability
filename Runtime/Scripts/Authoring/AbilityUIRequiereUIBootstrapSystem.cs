using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.UI;

public partial class AbilityAuthoring
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class AbilityUIRequiereUIBootstrapSystem : SystemBase
    {
        Dictionary<uint, AbilityUiLink> uiMap;

        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = false;
            Addressables.LoadAssetsAsync<AbilityUiLink>(new AssetLabelReference()
            {
                labelString = AbilityHelper.ADDRESSABLE_UiLink_LABEL
            }, null, false).Completed += objects =>
            {
                if (objects.Result == null) return;
                uiMap = new Dictionary<uint, AbilityUiLink>();
                foreach (AbilityUiLink uiLink in objects.Result)
                {
                    Debug.Log($"boostrap {uiLink.Id}");
                    uiMap.Add(uiLink.Id, uiLink);
                }

                Enabled = true;
            }; ;
        }

        protected override void OnUpdate()
        {

            Entities.WithStructuralChanges().ForEach((Entity entity, ref RequiereUIBootstrap boostrap, in DynamicBuffer<AbilityBufferElement> abilities) =>
            {

                if (uiMap.TryGetValue(boostrap.uiAssetGuid, out var link))
                {
                    UIDocument uiDoc = Instantiate(link.UiPrefab).GetComponent<UIDocument>();
                    if (uiDoc != null)
                    {
                        uiDoc.panelSettings = link.PanelSettings;
                        uiDoc.visualTreeAsset = link.UxmlUi;
                        uiDoc.sortingOrder = link.SortingOrder;
                        AbilityBookUIElement book = uiDoc.rootVisualElement.Q<AbilityBookUIElement>();
                        if (book != null) book.Populate(abilities, entity, EntityManager);
                        CastBar CastBar = uiDoc.rootVisualElement.Q<CastBar>();
                        if (CastBar != null) CastBar.SetOwnership(entity, EntityManager);
                    }
                }
                EntityManager.RemoveComponent<RequiereUIBootstrap>(entity);
            }).WithoutBurst().Run();
        }
    }
}
