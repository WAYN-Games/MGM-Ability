using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Skill.Demo
{
    public struct Effect1 : IEffect
    {
        public int Value;

        public void Convert(Entity entity, EntityManager dstManager, int skillIndex)
        {
            EffectUtility.AddEffect<Effect1Buffer, Effect1>(entity, dstManager, skillIndex, this);
        }
    }

    public struct Effect1Buffer : IEffectBufferElement<Effect1>
    {
        public int SkillIndex { get; set; }
        public Effect1 Effect { get; set; }
    }

    public struct Effect1Context : IEffectContext<Effect1>
    {
        public Entity Target { get; set; }
        public Effect1 Effect { get; set; }
    }

    [UpdateBefore(typeof(Effect1ConsumerSystem))]
    public class Effect1TriggerSystem : EffectTriggerSystem<Effect1Buffer, Effect1, Effect1ConsumerSystem, Effect1TriggerSystem.EffectWriter, Effect1Context>
    {

        [BurstCompile]
        public struct EffectWriter : IEffectContextWriter<Effect1>
        {

            public void PrepareChunk(ArchetypeChunk chunk)
            {
                // No additional data needed for this effect context.
            }

            public void WriteContextualizedEffect(int entityIndex, ref NativeStream.Writer consumerWriter, Effect1 effect, Entity target)
            {
                consumerWriter.Write(new Effect1Context() { Target = target, Effect = effect });
            }

        }

        protected override EffectWriter GetContextWriter()
        {
            return new EffectWriter()
            {
            };
        }
    }



    [UpdateAfter(typeof(Effect1TriggerSystem))]
    public class Effect1ConsumerSystem : EffectConsumerSystem<Effect1, Effect1Context>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, Effect1Context> effects = Effects;
            Entities.WithReadOnly(effects).ForEach((ref Entity entity, ref Health health) =>
            {
                NativeMultiHashMap<Entity, Effect1Context>.Enumerator effectEnumerator = effects.GetValuesForKey(entity);

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
