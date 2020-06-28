using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

using WaynGroup.Mgm.Ability;


[DisallowMultipleComponent]
[RequiresEntityConversion]
public class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<ScriptableAbility> Abilities;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Add a buffer to the entity and populate it with all the ability the entity can use.
        DynamicBuffer<AbilityBuffer> abilityBuffer = dstManager.AddBuffer<AbilityBuffer>(entity);
        for (int i = 0; i < Abilities.Count; i++)
        {
            if (Abilities[i] == null)
            {
                Debug.LogError($"Ability #{i} is missing reference on Game Object {name}");
                continue;
            }
            Ability Ability = new Ability(Abilities[i].CoolDown, Abilities[i].CastTime, Abilities[i].Range);
            abilityBuffer.Add(new AbilityBuffer()
            {
                Ability = Ability
            });
        }

        // Foreach ability the entity can use, register the ability's effect in one buffer per type of effect.
        // Each effect is linked to it's ability index in the ability buffer.
        for (int i = 0; i < Abilities.Count; i++)
        {
            if (Abilities[i] == null) continue;

            foreach (IEffect effect in Abilities[i].Effects)
            {
                effect.Convert(entity, dstManager, i);
            }
            foreach (IAbilityCost cost in Abilities[i].Costs)
            {
                cost.Convert(entity, dstManager, i);
            }
        }
    }
}
