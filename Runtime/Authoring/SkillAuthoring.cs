using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

using WaynGroup.Mgm.Ability;


[DisallowMultipleComponent]
[RequiresEntityConversion]
public class AbilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<ScriptableAbility> Abilitys;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Add a buffer to the entity and populate it with all the ability the entity can use.
        DynamicBuffer<AbilityBuffer> abilityBuffer = dstManager.AddBuffer<AbilityBuffer>(entity);
        for (int i = 0; i < Abilitys.Count; i++)
        {
            if (Abilitys[i] == null)
            {
                Debug.LogError($"Ability #{i} is missing reference on Game Object {name}");
                continue;
            }
            Ability Ability = new Ability(Abilitys[i].CoolDown, Abilitys[i].CastTime, Abilitys[i].Range);
            abilityBuffer.Add(new AbilityBuffer()
            {
                Ability = Ability
            });
        }

        // Foreach ability the entity can use, register the ability's effect in one buffer per type of effect.
        // Each effect is linked to it's ability index in the ability buffer.
        for (int i = 0; i < Abilitys.Count; i++)
        {
            if (Abilitys[i] == null) continue;

            foreach (IEffect effect in Abilitys[i].Effects)
            {
                effect.Convert(entity, dstManager, i);
            }
            foreach (IAbilityCost cost in Abilitys[i].Costs)
            {
                cost.Convert(entity, dstManager, i);
            }
        }
    }
}
