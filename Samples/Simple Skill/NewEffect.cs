using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using UnityEngine;

using WaynGroup.Mgm.Skill;

namespace NAMESAPCE
{
    public struct NewEffect : IEffect
    {
        [field: SerializeField] public EffectAffectType Affects { get; set; }

        // YOUR CODE : delcare all necesasry data inherant to the effect consumption, could be a the effect power, damage type,...



        // Mandatory for Authoring, do not edit
        public void Convert(Entity entity, EntityManager dstManager, int skillIndex)
        {
            EffectUtility.AddEffect<NewEffectBuffer, NewEffect>(entity, dstManager, skillIndex, this);
        }
    }

    // Mandatory for Authoring, do not edit
    public struct NewEffectBuffer : IEffectBufferElement<NewEffect>
    {
        public int SkillIndex { get; set; }
        public NewEffect Effect { get; set; }
    }

    public struct NewEffectContext : IEffectContext<NewEffect>
    {
        // YOUR CODE : delcare all necesasry contextual data for the effect consumption, could be a position, attack power,...



        // Mandatory for Authoring, do not edit
        public Entity Target { get; set; }
        public NewEffect Effect { get; set; }
    }

    public class NewEffectTriggerSystem : EffectTriggerSystem<NewEffectBuffer, NewEffect, NewEffectConsumerSystem, NewEffectTriggerSystem.TargetEffectWriter, NewEffectContext>
    {

        [BurstCompile]
        public struct TargetEffectWriter : IEffectContextWriter<NewEffect>
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
            public void WriteContextualizedEffect(int entityIndex, ref NativeStream.Writer consumerWriter, NewEffect effect, Entity target)
            {
                consumerWriter.Write(new NewEffectContext()
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

    public class NewEffectConsumerSystem : EffectConsumerSystem<NewEffect, NewEffectContext>
    {
        protected override void Consume()
        {
            NativeMultiHashMap<Entity, NewEffectContext> effects = _effects;
            Entities.WithReadOnly(effects).ForEach((ref Entity targetEntity/* YOUR CODE : component on the target that are nedded to apply the effect*/) =>
            {
                NativeMultiHashMap<Entity, NewEffectContext>.Enumerator effectEnumerator = effects.GetValuesForKey(targetEntity);


                while (effectEnumerator.MoveNext())
                {
                    // YOUR CODE : Consume the effect
                }

            }).WithBurst()
            .ScheduleParallel();
        }
    }

}
