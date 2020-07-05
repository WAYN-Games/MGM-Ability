using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace WaynGroup.Mgm.Ability.Demo
{
    /// <summary>
    /// System that fake the user input simulating all ability cast every frame.
    /// </summary>
    [UpdateBefore(typeof(AbilityUpdateSystemGroup))]
    [UpdateInGroup(typeof(AbilitySystemGroup))]
    public class UserInputSimulationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref DynamicBuffer<AbilityBuffer> abilityBuffer) =>
            {
                NativeArray<AbilityBuffer> sbArray = abilityBuffer.AsNativeArray();
                for (int i = 0; i < sbArray.Length; i++)
                {
                    Ability Ability = sbArray[i];
                    Ability.TryCast();
                    sbArray[i] = Ability;
                }


            }).WithBurst()
            .ScheduleParallel();
        }
    }
}
