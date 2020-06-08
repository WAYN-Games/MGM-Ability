using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace WaynGroup.Mgm.Skill
{
    /// <summary>
    /// This system update each skill timmings (cast and cooldown).
    /// Once a timing is elapsed, the skill state is updated to the propoer value.
    /// </summary>
    public class SkillUpdateSystem : SystemBase
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
                    if (Skill.State == SkillState.CoolingDown)
                    {
                        Skill.UpdateCoolDowns(DeltaTime);
                    }
                    if (Skill.State == SkillState.Casting)
                    {
                        Skill.UpdateCastTime(DeltaTime);
                    }
                    sbArray[i] = Skill;
                }
            }).WithBurst()
            .ScheduleParallel(Dependency);
        }

    }

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
