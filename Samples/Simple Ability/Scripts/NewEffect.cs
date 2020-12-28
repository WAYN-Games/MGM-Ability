using Unity.Collections;
using Unity.Entities;

using UnityEngine;

using WaynGroup.Mgm.Ability;

namespace NAMESAPCE
{
    public struct NewEffect : IEffect
    {
        [field: SerializeField] public TargetingMode Affects { get; set; }

        // YOUR CODE : delcare all necesasry data inherant to the effect consumption, could be a the effect power, damage type,...

    }

    public struct NewEffectContext : IEffectContext
    {
        // YOUR CODE : delcare all necesasry contextual data for the effect consumption, could be a position, attack power,...
    }

    public class NewEffectTriggerSystem : AbilityEffectTriggerSystem<NewEffect, NewEffectConsumerSystem, NewEffectTriggerSystem.TargetEffectWriter, NewEffectContext>
    {
        public struct TargetEffectWriter : IEffectContextWriter<NewEffectContext>
        {
            // YOUR CODE : declare the public [ReadOnly] component data chunk accessor and the private [ReadOnly] native array to cache the component data

            /// <summary>
            /// Cache the component data array needed to write the effect context.
            /// </summary>
            /// <param name="chunk"></param>
            public void PrepareChunk(ArchetypeChunk chunk)
            {
                // YOUR CODE : cache the component data array in a private [ReadOnly] field on the struct
            }

            /// <summary>
            /// Write the contextualized effect to it's corresponding consumer stream.
            /// </summary>
            /// <param name="entityIndex">The casting entity.</param>
            /// <param name="consumerWriter">The corresponding effect consumer stream.</param>
            /// <param name="effect">The effect to contextualize.</param>
            public NewEffectContext BuildEffectContext(int entityIndex)
            {
                return default;
            }
        }

    }

    public class NewEffectConsumerSystem : AbilityEffectConsumerSystem<NewEffect, NewEffectContext>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, ContextualizedEffect> effects = _effects;
            Entities.WithReadOnly(effects).ForEach((ref Entity targetEntity/* YOUR CODE : component on the target that are nedded to apply the effect*/) =>
            {
                NativeMultiHashMap<Entity, ContextualizedEffect>.Enumerator effectEnumerator = effects.GetValuesForKey(targetEntity);


                while (effectEnumerator.MoveNext())
                {
                    // YOUR CODE : Consume the effect
                }

            }).WithBurst()
            .ScheduleParallel();
        }
    }

}
