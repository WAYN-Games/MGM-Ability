using Unity.Entities;
using UnityEngine;

namespace WaynGroup.Mgm.Ability
{
    [UpdateAfter(typeof(AbilityConsumerSystemGroup))]
    public partial class AbilityCostCheckInitializationSystem : SystemBase
    {
        #region Protected Methods

        protected override void OnUpdate()
        {
            Entities.ForEach((ref AbilityInput abilityInput) =>
            {
                abilityInput.Disable();
            }).WithBurst()
                .ScheduleParallel();
        }

        #endregion Protected Methods
    }
}