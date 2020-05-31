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

    [UpdateBefore(typeof(Effect1ConsumerSystem))]
    public class Effect1TriggerSystem : EffectTriggerSystem<Effect1Buffer, Effect1, Effect1ConsumerSystem>
    {
    }

    [UpdateAfter(typeof(Effect1TriggerSystem))]
    public class Effect1ConsumerSystem : EffectConsumerSystem<Effect1>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, Effect1> effects = GetEffects();
            Dependency = Entities.WithBurst().WithReadOnly(effects).ForEach((ref Entity entity, ref Health health) =>
            {
                NativeMultiHashMap<Entity, Effect1>.Enumerator effectEnumerator = effects.GetValuesForKey(entity);

                Health hp = health;
                while (effectEnumerator.MoveNext())
                {

                    hp.Value -= effectEnumerator.Current.Value;

                }
                health = hp;
            }).ScheduleParallel(Dependency);
        }

    }
}
