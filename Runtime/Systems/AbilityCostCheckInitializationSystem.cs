using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(AbilityCostsSystemGroup), OrderFirst = true)]
    public class AbilityCostCheckInitializationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref DynamicBuffer<AbilityBufferElement> abilityBuffer) =>
            {
                for (int abilityIndex = 0; abilityIndex < abilityBuffer.Length; ++abilityIndex)
                {
                    AbilityBufferElement ability = abilityBuffer[abilityIndex];
                    ability.HasEnougthRessource = true;
                    abilityBuffer[abilityIndex] = ability;
                }
            }).WithBurst()
                .ScheduleParallel();

        }
    }


}
