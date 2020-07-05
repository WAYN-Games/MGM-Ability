using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// This system put the ability in cooldown if it's effect were triggered.
    /// It's run after all effect have been trigered.
    /// </summary>
    //[UpdateInGroup(typeof(AbilityTriggerSystemGroup), OrderLast = true)] // --> Does not actually work, the system is ordered first...
    [UpdateAfter(typeof(AbilityTriggerSystemGroup))]
    [UpdateInGroup(typeof(AbilitySystemGroup))]
    public class AbilityDeactivationSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            Dependency = Entities.ForEach((ref DynamicBuffer<AbilityBuffer> abilityBuffer) =>
            {
                NativeArray<AbilityBuffer> sbArray = abilityBuffer.AsNativeArray();
                for (int i = 0; i < sbArray.Length; i++)
                {
                    Ability Ability = sbArray[i];

                    if (Ability.State == AbilityState.Active)
                    {
                        Ability.StartCooloingDown();
                    }

                    sbArray[i] = Ability;
                }
            }).WithBurst()
            .ScheduleParallel(Dependency);
        }
    }
}
