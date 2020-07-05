using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// This system will determine if the current target is in range of a ability.
    /// </summary>
    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    public class AbilityUpdateRangeSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            ComponentDataFromEntity<Translation> translations = GetComponentDataFromEntity<Translation>(true);
            Dependency = Entities.WithReadOnly(translations).ForEach((ref Entity caster, ref DynamicBuffer<AbilityBuffer> abilityBuffer, ref Target target) =>
            {
                if (!translations.HasComponent(caster) || !translations.HasComponent(target.Value)) return;

                NativeArray<AbilityBuffer> sbArray = abilityBuffer.AsNativeArray();
                for (int i = 0; i < sbArray.Length; i++)
                {
                    Ability Ability = sbArray[i];
                    float distance = math.distance(translations[caster].Value, translations[target.Value].Value);
                    Ability.IsInRange = Ability.Range.Min <= distance && distance <= Ability.Range.Max;
                    sbArray[i] = Ability;
                }
            }).WithBurst()
            .ScheduleParallel(Dependency);
        }

    }
}
