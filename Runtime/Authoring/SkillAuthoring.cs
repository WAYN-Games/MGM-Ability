using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

using WaynGroup.Mgm.Skill;


[DisallowMultipleComponent]
[RequiresEntityConversion]
public class SkillAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    private List<ScriptableSkill> Skills;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        DynamicBuffer<SkillBuffer> skillBuffer = dstManager.AddBuffer<SkillBuffer>(entity);
        for (int i = 0; i < Skills.Count; i++)
        {
            if (Skills[i] == null)
            {
                Debug.LogError($"Skill #{i} is missing reference on Game Object {name}");
                continue;
            }
            Skill Skill = new Skill(Skills[i].CoolDown, Skills[i].CastTime, i);
            skillBuffer.Add(new SkillBuffer()
            {
                Skill = Skill
            });
        }

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
