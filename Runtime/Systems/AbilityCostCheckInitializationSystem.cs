using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    [UpdateBefore(typeof(AbilityUpdateSystemGroup))]
    [UpdateInGroup(typeof(AbilitySystemGroup))]
    public class AbilityCostCheckInitializationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref DynamicBuffer<AbilityBuffer> abilityBuffer) =>
            {
                for (int abilityIndex = 0; abilityIndex < abilityBuffer.Length; ++abilityIndex)
                {
                    Ability ability = abilityBuffer[abilityIndex];
                    ability.HasEnougthRessource = true;
                    abilityBuffer[abilityIndex] = ability;
                }
            }).WithBurst()
                .ScheduleParallel();
        }
    }


}
