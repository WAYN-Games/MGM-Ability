using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace WaynGroup.Mgm.Skill
{
    public class SkillUpdateSystemGroup : ComponentSystemGroup
    {

    }



    /// <summary>
    /// This system update each skill timmings (cast and cooldown).
    /// Once a timing is elapsed, the skill state is updated to the proper value.
    /// </summary>
    [UpdateInGroup(typeof(SkillUpdateSystemGroup))]
    public class SkillUpdateTimingsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float DeltaTime = World.Time.DeltaTime;
            Dependency = Entities.ForEach((ref DynamicBuffer<SkillBuffer> skillBuffer) =>
            {
                NativeArray<SkillBuffer> sbArray = skillBuffer.AsNativeArray();
                for (int i = 0; i < sbArray.Length; i++)
                {

                    Skill Skill = sbArray[i];
                    if (Skill.State == SkillState.Casting)
                    {
                        Skill.UpdateCastTime(DeltaTime);
                    }
                    if (Skill.State == SkillState.CoolingDown)
                    {
                        Skill.UpdateCoolDowns(DeltaTime);
                    }

                    sbArray[i] = Skill;
                }
            }).WithBurst()
            .ScheduleParallel(Dependency);

        }
    }

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

    /// <summary>
    /// This system put the skill in cooldown.
    /// It's run after all effect have beed trigered.
    /// </summary>
    public class SkillDeactivationSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            Dependency = Entities.ForEach((ref DynamicBuffer<SkillBuffer> skillBuffer) =>
            {
                NativeArray<SkillBuffer> sbArray = skillBuffer.AsNativeArray();
                for (int i = 0; i < sbArray.Length; i++)
                {
                    Skill Skill = sbArray[i];
                    if (Skill.State == SkillState.Active)
                    {
                        Skill.Deactivate();
                    }
                    sbArray[i] = Skill;
                }
            }).WithBurst()
            .ScheduleParallel(Dependency);
        }
    }
}
