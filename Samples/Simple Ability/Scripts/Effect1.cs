using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace WaynGroup.Mgm.Ability.Demo
{
    public struct Effect1 : IEffect
    {
        public int Value;
        [field: SerializeField] public TargetingMode Affects { get; set; }
    }

    public struct Effect1Context : IEffectContext
    {
    }

    public class Effect1TriggerSystem : AbilityEffectTriggerSystem<Effect1, Effect1ConsumerSystem, Effect1TriggerSystem.EffectWriter, Effect1Context>
    {
        public struct EffectWriter : IEffectContextWriter<Effect1Context>
        {
            public void PrepareChunk(ArchetypeChunk chunk)
            {
                // No contextual data needed for this effect context.
            }

            public Effect1Context BuildEffectContext(int entityIndex)
            {
                return default;
            }
        }

    }

    public class Effect1ConsumerSystem : AbilityEffectConsumerSystem<Effect1, Effect1Context>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, ContextualizedEffect> effects = _effects;
            Entities.WithReadOnly(effects).ForEach((ref Entity entity, ref Health health) =>
            {
                NativeMultiHashMap<Entity, ContextualizedEffect>.Enumerator effectEnumerator = effects.GetValuesForKey(entity);
                Health hp = health;
                while (effectEnumerator.MoveNext())
                {
                    hp.Value -= effectEnumerator.Current.Effect.Value;
                }
                health = hp;

            }).WithBurst()
            .ScheduleParallel();
        }
    }
}
