using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

using WaynGroup.Mgm.Skill;


[DisallowMultipleComponent]
[RequiresEntityConversion]
public class SkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public List<ScriptableSkill> Skills;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Add a buffer to the entity and populate it with all the skill the entity can use.
        DynamicBuffer<SkillBuffer> skillBuffer = dstManager.AddBuffer<SkillBuffer>(entity);
        for (int i = 0; i < Skills.Count; i++)
        {
            if (Skills[i] == null)
            {
                Debug.LogError($"Skill #{i} is missing reference on Game Object {name}");
                continue;
            }
            Skill Skill = new Skill(Skills[i].CoolDown, Skills[i].CastTime, Skills[i].Range);
            skillBuffer.Add(new SkillBuffer()
            {
                Skill = Skill
            });
        }

        // Foreach skill the entity can use, register the skill's effect in one buffer per type of effect.
        // Each effect is linked to it's skill index in the skill buffer.
        for (int i = 0; i < Skills.Count; i++)
        {
            if (Skills[i] == null) continue;

            foreach (IEffect effect in Skills[i].Effects)
            {
                effect.Convert(entity, dstManager, i);
            }
        }
    }
}
