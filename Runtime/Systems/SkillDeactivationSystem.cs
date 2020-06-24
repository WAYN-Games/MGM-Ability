using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace WaynGroup.Mgm.Skill
{
    /// <summary>
    /// This system put the skill in cooldown if it's effect were triggered.
    /// It's run after all effect have been trigered.
    /// </summary>
    //[UpdateInGroup(typeof(SkillTriggerSystemGroup), OrderLast = true)] // --> Does not actually work, the system is ordered first...
    [UpdateAfter(typeof(SkillTriggerSystemGroup))]
    [UpdateInGroup(typeof(SkillSystemsGroup))]
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
                        Skill.StartCooloingDown();
                    }

                    sbArray[i] = Skill;
                }
            }).WithBurst()
            .ScheduleParallel(Dependency);
        }
    }
}
