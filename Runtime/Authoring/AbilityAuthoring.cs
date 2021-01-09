using System;
using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;
using UnityEngine.AddressableAssets;

using WaynGroup.Mgm.Ability;


[DisallowMultipleComponent]
[RequiresEntityConversion]
public class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Intial time needed for all abilities to be ready. This allow the system to initialise before an ability can be activated.")]
    public float InitialGlobalCoolDown = 2.5f;

    [Tooltip("List of Scriptable Ability addressable asset reference.")]
    public List<AssetReferenceT<ScriptableAbility>> Abilities;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        AddAbilityReferences(entity, dstManager);
    }

    private void AddAbilityReferences(Entity entity, EntityManager dstManager)
    {
        DynamicBuffer<AbilityBufferElement> abilityBuffer = dstManager.AddBuffer<AbilityBufferElement>(entity);
        for (int i = 0; i < Abilities.Count; i++)
        {
            abilityBuffer.Add(new AbilityBufferElement()
            {
                Guid = new Guid(Abilities[i].AssetGUID),
                CurrentTimming = InitialGlobalCoolDown,
                AbilityState = AbilityState.CoolingDown
            });
        }
    }
}
