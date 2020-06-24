using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace WaynGroup.Mgm.Skill
{
    /// <summary>
    /// This system will determine if the current target is in range of a skill.
    /// </summary>
    [UpdateInGroup(typeof(SkillUpdateSystemGroup))]
    public class SkillUpdateRangeSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            ComponentDataFromEntity<Translation> translations = GetComponentDataFromEntity<Translation>(true);
            Dependency = Entities.WithReadOnly(translations).ForEach((ref Entity caster, ref DynamicBuffer<SkillBuffer> skillBuffer, ref Target target) =>
            {
                if (!translations.HasComponent(caster) || !translations.HasComponent(target.Value)) return;

                NativeArray<SkillBuffer> sbArray = skillBuffer.AsNativeArray();
                for (int i = 0; i < sbArray.Length; i++)
                {
                    Skill Skill = sbArray[i];
                    float distance = math.distance(translations[caster].Value, translations[target.Value].Value);
                    Skill.IsInRange = Skill.Range.Min <= distance && distance <= Skill.Range.Max;
                    sbArray[i] = Skill;
                }
            }).WithBurst()
            .ScheduleParallel(Dependency);
        }

    }
}
