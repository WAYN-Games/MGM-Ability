using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using WaynGroup.Mgm.Ability;

[DisallowMultipleComponent]
public partial class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Intial time used for all abilities to be ready in case of runtime conversion. (The subscene workflow is prefered).")]
    public float InitialGlobalCoolDown = 2.5f;

    [Tooltip("List of Scriptable Ability addressable asset reference.")]
    public List<AssetReferenceT<ScriptableAbility>> Abilities;

    [Tooltip("Will look for that name in the first UIDocument found in scene to bind the UI data. Note that this name is limited to the length of a FixedString64.")]
    public AssetReferenceT<AbilityUiLink> AbilityUiLink;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<AbilityInput>(entity);
        dstManager.AddComponent<CurrentlyCasting>(entity);
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

        {

            if (AbilityUiLink != null)
            {
#if UNITY_EDITOR
                conversionSystem.DeclareAssetDependency(gameObject, AbilityUiLink.editorAsset);
#endif

                dstManager.AddComponentData(entity, new RequiereUIBootstrap()
                {
                    uiAssetGuid = AbilityHelper.ComputeAbilityIdFromGuid(AbilityUiLink.AssetGUID)
                });
            }

        }
    }

    public struct RequiereUIBootstrap : IComponentData
    {
        public uint uiAssetGuid;
    }
}
