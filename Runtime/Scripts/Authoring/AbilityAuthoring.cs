using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;
using UnityEngine.AddressableAssets;

using WaynGroup.Mgm.Ability;


[DisallowMultipleComponent]
public class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Intial time used for all abilities to be ready in case of runtime conversion. (The subscene workflow is prefered).")]
    public float InitialGlobalCoolDown = 2.5f;

    [Tooltip("List of Scriptable Ability addressable asset reference.")]
    public List<AssetReferenceT<ScriptableAbility>> Abilities;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
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
    }



}
