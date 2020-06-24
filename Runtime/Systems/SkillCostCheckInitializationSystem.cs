using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    [UpdateInGroup(typeof(SkillUpdateSystemGroup), OrderFirst = true)]
    public class SkillCostCheckInitializationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref DynamicBuffer<SkillBuffer> skillBuffer) =>
            {
                for (int skillIndex = 0; skillIndex < skillBuffer.Length; ++skillIndex)
                {
                    Skill skill = skillBuffer[skillIndex];
                    skill.HasEnougthRessource = true;
                    skillBuffer[skillIndex] = skill;
                }
            }).WithBurst()
                .ScheduleParallel();
        }
    }


}
