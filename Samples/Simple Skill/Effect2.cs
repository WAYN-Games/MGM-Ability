using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Skill.Demo
{
    public struct Effect2 : IEffect
    {
        public int Value;

        public void Convert(Entity entity, EntityManager dstManager, int skillIndex)
        {
            EffectUtility.AddEffect<Effect2Buffer, Effect2>(entity, dstManager, skillIndex, this);
        }
    }

    public struct Effect2Buffer : IEffectBufferElement<Effect2>
    {
        public int SkillIndex { get; set; }
        public Effect2 Effect { get; set; }
    }


    [UpdateBefore(typeof(Effect2ConsumerSystem))]
    public class Effect2TriggerSystem : EffectTriggerSystem<Effect2Buffer, Effect2, Effect2ConsumerSystem>
    {

    }

    [UpdateAfter(typeof(Effect2TriggerSystem))]
    public class Effect2ConsumerSystem : EffectConsumerSystem<Effect2>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, Effect2> effects = GetEffects();
            Dependency = Entities.WithReadOnly(effects).ForEach((ref Entity entity, ref Health health) =>
            {

                NativeMultiHashMap<Entity, Effect2>.Enumerator effectEnumerator = effects.GetValuesForKey(entity);
                Health hp = health;
                while (effectEnumerator.MoveNext())
                {

                    hp.Value -= effectEnumerator.Current.Value;

                }
                health = hp;
            }).WithBurst()
            .ScheduleParallel(Dependency);
        }

    }

}
