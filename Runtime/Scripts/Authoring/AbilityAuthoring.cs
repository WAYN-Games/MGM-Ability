using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;
using UnityEngine.AddressableAssets;

using WaynGroup.Mgm.Ability;


[DisallowMultipleComponent]
public class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Intial time needed for all abilities to be ready. This allow the system to initialise before an ability can be activated.")]
    public float InitialGlobalCoolDown = 2.5f;

    [Tooltip("List of Scriptable Ability addressable asset reference.")]
    public List<AssetReferenceT<ScriptableAbility>> Abilities;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        DynamicBuffer<AbilityBufferElement> abilityBuffer = dstManager.AddBuffer<AbilityBufferElement>(entity);
        for (int i = 0; i < Abilities.Count; i++)
        {
            conversionSystem.DeclareAssetDependency(gameObject, Abilities[i].editorAsset);
            AbilityHelper.AddAbility(Abilities[i].editorAsset, ref abilityBuffer);
        }
    }



}
