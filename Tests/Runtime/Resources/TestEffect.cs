using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using WaynGroup.Mgm.Ability.Tests;

namespace WaynGroup.Mgm.Ability
{
    public struct TestEffect : IEffect
    {
        public EffectAffectType Affects { get; set; }

        // YOUR CODE : delcare all necesasry data inherant to the effect consumption, could be a the effect power, damage type,...

        public int Value;

        // Mandatory for Authoring, do not edit
        public void Convert(Entity entity, EntityManager dstManager, int abilityIndex)
        {
            EffectUtility.AddEffect<TestEffectBuffer, TestEffect>(entity, dstManager, abilityIndex, this);
        }
    }

    // Mandatory for Authoring, do not edit
    public struct TestEffectBuffer : IEffectBufferElement<TestEffect>
    {
        public int AbilityIndex { get; set; }
        public TestEffect Effect { get; set; }
    }

    public struct TestEffectContext : IEffectContext<TestEffect>
    {
        // YOUR CODE : delcare all necesasry contextual data for the effect consumption, could be a position, attack power,...
        public float testResourceValue;


        // Mandatory for Authoring, do not edit
        public Entity Target { get; set; }
        public TestEffect Effect { get; set; }
    }

    [DisableAutoCreation]
    public class TestEffectTriggerSystem : AbilityEffectTriggerSystem<TestEffectBuffer, TestEffect, TestEffectConsumerSystem, TestEffectTriggerSystem.TargetEffectWriter, TestEffectContext>
    {

        [BurstCompile]
        public struct TargetEffectWriter : IEffectContextWriter<TestEffect>
        {
            // YOUR CODE : declare the public [ReadOnly] component data chunk accessor and the private [ReadOnly] native array to cache the component data
            [ReadOnly] public ComponentTypeHandle<TestResource> TestResourceChunk;
            [ReadOnly] private NativeArray<TestResource> _testResources;
            /// <summary>
            /// Cache the component data array needed to write the effect context.
            /// </summary>
            /// <param name="chunk"></param>
            public void PrepareChunk(ArchetypeChunk chunk)
            {
                _testResources = chunk.GetNativeArray(TestResourceChunk);
                // YOUR CODE : cache the component data array in a private [ReadOnly] field on the struct
            }

            /// <summary>
            /// Write the contextualized effect to it's corresponding consumer stream.
            /// </summary>
            /// <param name="entityIndex">The casting entity.</param>
            /// <param name="consumerWriter">The corresponding effect consumer stream.</param>
            /// <param name="effect">The effect to contextualize.</param>
            public void WriteContextualizedEffect(int entityIndex, ref NativeStream.Writer consumerWriter, TestEffect effect, Entity target)
            {
                consumerWriter.Write(new TestEffectContext()
                {
                    Target = target,
                    Effect = effect,
                    testResourceValue = _testResources[entityIndex].Value
                    // YOUR CODE : populate the effect context with additonal contextual data.
                });
            }
        }


        protected override TargetEffectWriter GetContextWriter()
        {
            return new TargetEffectWriter()
            {
                TestResourceChunk = GetComponentTypeHandle<TestResource>(true)
            };
        }

        protected override EntityQueryDesc GetEffectContextEntityQueryDesc()
        {
            return new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                     ComponentType.ReadOnly<TestResource>()
                }
            };
        }

    }

    [DisableAutoCreation]
    public class TestEffectConsumerSystem : AbilityEffectConsumerSystem<TestEffect, TestEffectContext>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, TestEffectContext> effects = _effects;
            Entities.WithReadOnly(effects).ForEach((ref Entity targetEntity, ref TestResource resource) =>
            {
                NativeMultiHashMap<Entity, TestEffectContext>.Enumerator effectEnumerator = effects.GetValuesForKey(targetEntity);

                while (effectEnumerator.MoveNext())
                {
                    resource.Value -= effectEnumerator.Current.Effect.Value;
                }

            }).WithBurst()
            .ScheduleParallel();
        }
    }

}
