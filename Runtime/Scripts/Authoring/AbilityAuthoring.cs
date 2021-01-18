using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.UI;

[DisallowMultipleComponent]
public class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Intial time used for all abilities to be ready in case of runtime conversion. (The subscene workflow is prefered).")]
    public float InitialGlobalCoolDown = 2.5f;

    [Tooltip("List of Scriptable Ability addressable asset reference.")]
    public List<AssetReferenceT<ScriptableAbility>> Abilities;

    [Tooltip("Will look for that name in the first UIDocument found in scene to bind the UI data. Note that this name is limited to the length of a FixedString64.")]
    public string UiElementName;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<AbilityInput>(entity);
        DynamicBuffer<AbilityBufferElement> abilityBuffer = dstManager.AddBuffer<AbilityBufferElement>(entity);

        for (int i = 0; i < Abilities.Count; i++)
        {
#if UNITY_EDITOR
            conversionSystem.DeclareAssetDependency(gameObject, Abilities[i].editorAsset);
            AbilityHelper.AddAbility(Abilities[i].editorAsset, ref abilityBuffer);
#endif
#if !UNITY_EDITOR
            Debug.LogWarning($"Runtime conversion through AbilityAuthoring does not support asset loading. It will take default value instead");
            AbilityHelper.AddAbility(AbilityHelper.ComputeAbilityIdFromGuid(Abilities[i].AssetGUID), InitialGlobalCoolDown, ref abilityBuffer);
#endif

        }
        if (!string.IsNullOrEmpty(UiElementName))
        {
            if (UiElementName.Length > FixedString64.UTF8MaxLengthInBytes)
            {
                Debug.LogError($"'{UiElementName}' is too long, please edit the name so that it doesn't exceed {FixedString64.UTF8MaxLengthInBytes}");
            }
            dstManager.AddComponentData(entity, new RequiereUIBootstrap()
            {
                uiElementName = new FixedString64(UiElementName.Substring(0, math.min(UiElementName.Length, FixedString64.UTF8MaxLengthInBytes)))
            }
            );
        }
    }

    public struct RequiereUIBootstrap : IComponentData
    {
        public FixedString64 uiElementName;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class AbilityUIRequiereUIBootstrapSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            UIDocument uiDocument = FindObjectOfType<UIDocument>();
            Entities.WithStructuralChanges().ForEach((Entity entity, ref RequiereUIBootstrap boostrap, in DynamicBuffer<AbilityBufferElement> abilities) =>
            {
                foreach (AbilityBufferElement ability in abilities)
                {
                    uiDocument.rootVisualElement.Q<AbilityUIElement>(boostrap.uiElementName.ConvertToString()).AssignAbility(entity, ability.Guid, EntityManager);
                }
                EntityManager.RemoveComponent<RequiereUIBootstrap>(entity);
            }).WithoutBurst().Run();
        }
    }
}
