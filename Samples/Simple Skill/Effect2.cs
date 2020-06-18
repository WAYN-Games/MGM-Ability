using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

using UnityEngine;

namespace WaynGroup.Mgm.Skill.Demo
{
    public struct Effect2 : IEffect
    {
        // Actual effect data
        public int Value;

        [field: SerializeField] public EffectAffectType Affects { get; set; }

        // Mandatory for Authoring, do not edit
        public void Convert(Entity entity, EntityManager dstManager, int skillIndex)
        {
            EffectUtility.AddEffect<Effect2Buffer, Effect2>(entity, dstManager, skillIndex, this);
        }
    }

    // Mandatory for Authoring, do not edit
    public struct Effect2Buffer : IEffectBufferElement<Effect2>
    {
        public int SkillIndex { get; set; }
        public Effect2 Effect { get; set; }
    }

    public struct Effect2Context : IEffectContext<Effect2>
    {
        // Additional context data (generaly speaking, data comming from caster like position or attack power adn so on)
        public float AttackPower;

        // Mandatory for Authoring, do not edit
        public Entity Target { get; set; }
        public Effect2 Effect { get; set; }
    }

    public class Effect2TriggerSystem : EffectTriggerSystem<Effect2Buffer, Effect2, Effect2ConsumerSystem, Effect2TriggerSystem.TargetEffectWriter, Effect2Context>
    {

        [BurstCompile]
        public struct TargetEffectWriter : IEffectContextWriter<Effect2>
        {
            [ReadOnly] public ArchetypeChunkComponentType<AttackPower> chunkAttackPowers;

            // The array should be declared private as it's only use is within this structure and [ReadOnly] beacause we only write to the stream.
            // [ReadOnly] is not the c# 'readonly' modifer, the AttackPowers can be assigned but array it point ot can not be written to.
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
            public void WriteContextualizedEffect(int entityIndex, ref NativeStream.Writer consumerWriter, Effect2 effect, Entity target)
            {
                float attackPower = 1;
                if (_attackPowers.Length > entityIndex)
                {
                    attackPower = _attackPowers[entityIndex].Value;
                }

                consumerWriter.Write(new Effect2Context()
                {
                    AttackPower = attackPower,
                    Target = target,
                    Effect = effect
                });
            }
        }


        protected override TargetEffectWriter GetContextWriter()
        {
            return new TargetEffectWriter()
            {
                chunkAttackPowers = GetArchetypeChunkComponentType<AttackPower>(true)
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

    public class Effect2ConsumerSystem : EffectConsumerSystem<Effect2, Effect2Context>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, Effect2Context> effects = _effects;
            Entities.WithReadOnly(effects).ForEach((ref Entity entity, ref Health health, ref Armor armor) =>
            {
                NativeMultiHashMap<Entity, Effect2Context>.Enumerator effectEnumerator = effects.GetValuesForKey(entity);

                Health hp = health;
                while (effectEnumerator.MoveNext())
                {
                    hp.Value -= effectEnumerator.Current.AttackPower * effectEnumerator.Current.Effect.Value / armor.Value;
                }
                health = hp;

            }).WithBurst()
            .ScheduleParallel();
        }
    }

}
