using System;
using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

using WaynGroup.Mgm.Ability;


[DisallowMultipleComponent]
[RequiresEntityConversion]
public class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<AbilityAssignement> AbilityAssignements;

    public AbilityUI AbilityUI;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Add a buffer to the entity and populate it with all the ability the entity can use.
        DynamicBuffer<AbilityBuffer> abilityBuffer = dstManager.AddBuffer<AbilityBuffer>(entity);
        for (int i = 0; i < AbilityAssignements.Count; i++)
        {
            ScriptableAbility scriptableAbility = AbilityAssignements[i].Ability;
            if (scriptableAbility == null)
            {
                Debug.LogError($"Ability #{i} is missing reference on Game Object {name}");
                continue;
            }
            Ability Ability = new Ability(scriptableAbility.CoolDown, scriptableAbility.CastTime, scriptableAbility.Range);
            abilityBuffer.Add(new AbilityBuffer()
            {
                Ability = Ability
            });
            if (AbilityUI == null) continue;
            AbilityUI?.AssignAbility(entity, i, scriptableAbility, AbilityAssignements[i].Bar, AbilityAssignements[i].Slot, dstManager);
        }

        // Foreach ability the entity can use, register the ability's effect in one buffer per type of effect.
        // Each effect is linked to it's ability index in the ability buffer.
        for (int i = 0; i < AbilityAssignements.Count; i++)
        {
            ScriptableAbility scriptableAbility = AbilityAssignements[i].Ability;
            if (scriptableAbility == null) continue;

            foreach (IEffect effect in scriptableAbility.Effects)
            {
                effect.Convert(entity, dstManager, i);
            }
            foreach (IAbilityCost cost in scriptableAbility.Costs)
            {
                cost.Convert(entity, dstManager, i);
            }
        }
    }

}

[Serializable]
public class AbilityAssignement
{
    public ScriptableAbility Ability;
    public int Bar;
    public int Slot;
}
