using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;



namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// This system update each ability timmings (cast and cooldown).
    /// Once a timing is elapsed, the ability state is updated to the proper value.
    /// </summary>
    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    public class AbilityUpdateTimingsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float DeltaTime = World.Time.DeltaTime;
            Dependency = Entities.ForEach((ref DynamicBuffer<AbilityBuffer> abilityBuffer) =>
            {
                NativeArray<AbilityBuffer> sbArray = abilityBuffer.AsNativeArray();
                for (int i = 0; i < sbArray.Length; i++)
                {

                    Ability Ability = sbArray[i];
                    if (Ability.State == AbilityState.Casting)
                    {
                        Ability.UpdateCastTime(DeltaTime);
                    }
                    if (Ability.State == AbilityState.CoolingDown)
                    {
                        Ability.UpdateCoolDowns(DeltaTime);
                    }

                    sbArray[i] = Ability;
                }
            }).WithBurst()
            .ScheduleParallel(Dependency);

        }
    }
}
