using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace WaynGroup.Mgm.Ability.Demo
{
    public struct Effect2 : IEffect
    {
        // Actual effect data
        public int Value;

        [field: SerializeField] public TargetingMode Affects { get; set; }
    }

    public struct Effect2Context : IEffectContext
    {
        // Additional context data (generaly speaking, data comming from caster like position or attack power adn so on)
        public float AttackPower;
    }

    public class Effect2TriggerSystem : AbilityEffectTriggerSystem<Effect2, Effect2ConsumerSystem, Effect2TriggerSystem.TargetEffectWriter, Effect2Context>
    {
        public struct TargetEffectWriter : IEffectContextWriter<Effect2Context>
        {
            [ReadOnly] public ComponentTypeHandle<AttackPower> chunkAttackPowers;

            // The array should be declared private as it's only use is within this structure and [ReadOnly] beacause we don't write to it.
            // [ReadOnly] is not the c# 'readonly' modifier, the AttackPowers can be assigned but array it point to can not be written to.
            [ReadOnly] [NativeDisableContainerSafetyRestriction] private NativeArray<AttackPower> _attackPowers;

            /// <summary>
            /// Cache the component data array needed to write the effect context.
            /// </summary>
            /// <param name="chunk"></param>
            public void PrepareChunk(ArchetypeChunk chunk)
            {
                _attackPowers = chunk.GetNativeArray(chunkAttackPowers);
            }

            /// <summary>
            /// Write the contextualized effect to it's corresponding consumer stream.
            /// </summary>
            /// <param name="entityIndex">The casting entity.</param>
            /// <param name="consumerWriter">The corresponding effect consumer stream.</param>
            /// <param name="effect">The effect to contextualize.</param>
            public Effect2Context BuildEffectContext(int entityIndex)
            {
                return new Effect2Context()
                {
                    AttackPower = _attackPowers.Length > entityIndex ? _attackPowers[entityIndex].Value : 1
                };
            }
        }


        protected override TargetEffectWriter GetContextWriter()
        {
            return new TargetEffectWriter()
            {
                chunkAttackPowers = GetComponentTypeHandle<AttackPower>(true)
            };
        }

        protected override EntityQueryDesc GetEffectContextEntityQueryDesc()
        {
            return new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                        ComponentType.ReadOnly<AttackPower>(),
                }
            };
        }
    }

    public class Effect2ConsumerSystem : AbilityEffectConsumerSystem<Effect2, Effect2Context>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, ContextualizedEffect> effects = _effects;
            Entities.WithReadOnly(effects).ForEach((ref Entity entity, ref Health health, ref Armor armor) =>
            {
                NativeMultiHashMap<Entity, ContextualizedEffect>.Enumerator effectEnumerator = effects.GetValuesForKey(entity);

                Health hp = health;
                while (effectEnumerator.MoveNext())
                {
                    hp.Value -= effectEnumerator.Current.Context.AttackPower * effectEnumerator.Current.Effect.Value / armor.Value;
                }
                health = hp;

            }).WithBurst()
            .ScheduleParallel();
        }
    }

}
