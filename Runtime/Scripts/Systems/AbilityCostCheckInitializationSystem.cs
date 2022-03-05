using Unity.Entities;
using UnityEngine;

namespace WaynGroup.Mgm.Ability
{
    [UpdateAfter(typeof(AbilityConsumerSystemGroup))]
    public class AbilityCostCheckInitializationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref AbilityInput abilityInput) =>
            {
                abilityInput.Disable();
            }).WithBurst()
                .ScheduleParallel();
        }
    }


}
