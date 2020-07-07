using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Ability.Tests
{
    public struct TestEmptyEffect : IEffect
    {
        public EffectAffectType Affects { get; set; }

        // YOUR CODE : delcare all necesasry data inherant to the effect consumption, could be a the effect power, damage type,...



        // Mandatory for Authoring, do not edit
        public void Convert(Entity entity, EntityManager dstManager, int abilityIndex)
        {
            EffectUtility.AddEffect<TestEmptyEffectBuffer, TestEmptyEffect>(entity, dstManager, abilityIndex, this);
        }
    }

    // Mandatory for Authoring, do not edit
    public struct TestEmptyEffectBuffer : IEffectBufferElement<TestEmptyEffect>
    {
        public int AbilityIndex { get; set; }
        public TestEmptyEffect Effect { get; set; }
    }

    public struct TestEmptyEffectContext : IEffectContext<TestEmptyEffect>
    {
        // YOUR CODE : delcare all necesasry contextual data for the effect consumption, could be a position, attack power,...



        // Mandatory for Authoring, do not edit
        public Entity Target { get; set; }
        public TestEmptyEffect Effect { get; set; }
    }

    [DisableAutoCreation]
    public class TestEmptyEffectTriggerSystem : AbilityEffectTriggerSystem<TestEmptyEffectBuffer, TestEmptyEffect, TestEmptyEffectConsumerSystem, TestEmptyEffectTriggerSystem.TargetEffectWriter, TestEmptyEffectContext>
    {

        [BurstCompile]
        public struct TargetEffectWriter : IEffectContextWriter<TestEmptyEffect>
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
            public void WriteContextualizedEffect(int entityIndex, ref NativeStream.Writer consumerWriter, TestEmptyEffect effect, Entity target)
            {
                consumerWriter.Write(new TestEmptyEffectContext()
                {
                    Target = target,
                    Effect = effect
                    // YOUR CODE : populate the effect context with additonal contextual data.
                });
            }
        }


        protected override TargetEffectWriter GetContextWriter()
        {
            return new TargetEffectWriter()
            {
                // YOUR CODE : populate the component data chunk accessor
            };
        }

        /* Optional
        protected override EntityQueryDesc GetEffectContextEntityQueryDesc()
        {
            return new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                     // YOUR CODE : declare all required component type for populating the context of the effect.
                }
            };
        }
        */
    }

    [DisableAutoCreation]
    public class TestEmptyEffectConsumerSystem : AbilityEffectConsumerSystem<TestEmptyEffect, TestEmptyEffectContext>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, TestEmptyEffectContext> effects = _effects;
            Entities.WithReadOnly(effects).ForEach((ref Entity targetEntity/* YOUR CODE : component on the target that are nedded to apply the effect*/) =>
            {
                NativeMultiHashMap<Entity, TestEmptyEffectContext>.Enumerator effectEnumerator = effects.GetValuesForKey(targetEntity);


                while (effectEnumerator.MoveNext())
                {
                    // YOUR CODE : Consume the effect
                }

            }).WithBurst()
            .ScheduleParallel();
        }
    }

}
