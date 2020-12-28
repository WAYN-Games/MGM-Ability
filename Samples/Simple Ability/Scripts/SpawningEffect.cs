using Unity.Collections;
using Unity.Entities;

using WaynGroup.Mgm.Ability;

namespace NAMESAPCE
{

    public struct SpawningEffect : IEffect
    {
        public TargetingMode Affects { get; set; }
        // YOUR CODE : delcare all necesasry data inherant to the effect consumption, could be a the effect power, damage type,...
        public Entity prefabEntity;
        public int count;
    }

    public struct SpawningEffectContext : IEffectContext
    {
        // YOUR CODE : delcare all necesasry contextual data for the effect consumption, could be a position, attack power,...
    }

    public class SpawningEffectTriggerSystem : AbilityEffectTriggerSystem<SpawningEffect, SpawningEffectConsumerSystem, SpawningEffectTriggerSystem.TargetEffectWriter, SpawningEffectContext>
    {
        public struct TargetEffectWriter : IEffectContextWriter<SpawningEffectContext>
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
            public SpawningEffectContext BuildEffectContext(int entityIndex)
            {
                return default;
            }
        }

    }

    public class SpawningEffectConsumerSystem : AbilityEffectConsumerSystem<SpawningEffect, SpawningEffectContext>
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
