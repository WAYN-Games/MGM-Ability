using NUnit.Framework;

using Unity.Collections;
using Unity.Entities;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Tests;

public class AbilityEffectTriggerSystemTest
{
    public class WithoutAdditionnalContext : DotsTest
    {

        [Test]
        public void OnlyTriggerEffectOfActiveSkill([Values(AbilityState.CooledDown, AbilityState.CoolingDown, AbilityState.Casting)] AbilityState state)
        {
            // Arrange
            Entity caster = _entityManager.CreateEntity();

            _entityManager.AddComponentData(caster, new Target() { Value = Entity.Null });


            Ability ability = new Ability(1, 1, new Range())
            {
                State = AbilityState.Active
            };

            DynamicBuffer<AbilityBuffer> abilityBuffer = _entityManager.AddBuffer<AbilityBuffer>(caster);
            abilityBuffer.Add(new AbilityBuffer()
            {
                Ability = ability
            });
            abilityBuffer.Add(new AbilityBuffer()
            {
                Ability = new Ability(1, 1, new Range())
                {
                    State = state
                }
            });

            DynamicBuffer<TestEmptyEffectBuffer> testEffectBuffer = _entityManager.AddBuffer<TestEmptyEffectBuffer>(caster);
            testEffectBuffer.Add(new TestEmptyEffectBuffer()
            {
                AbilityIndex = 0,
                Effect = new TestEmptyEffect()
                {
                    Affects = EffectAffectType.Target
                }
            });
            testEffectBuffer.Add(new TestEmptyEffectBuffer()
            {
                AbilityIndex = 1,
                Effect = new TestEmptyEffect()
                {
                    Affects = EffectAffectType.Target
                }
            });

            _world.WithSystem<TestEmptyEffectTriggerSystem>();

            //Act
            _world.UpdateSystem<TestEmptyEffectTriggerSystem>();
            _world.CompleteAllJobs();

            //Assert
            TestEmptyEffectConsumerSystem conusmerSystem = _world.GetReference().GetExistingSystem<TestEmptyEffectConsumerSystem>();
            NativeStream.Reader reader = conusmerSystem.GetEffectReader();
            Assert.AreEqual(1, conusmerSystem.GetEffectReader().Count());
            reader.BeginForEachIndex(0);
            TestEmptyEffectContext ctx = reader.Read<TestEmptyEffectContext>();
            Assert.AreEqual(Entity.Null, ctx.Target);
            Assert.AreEqual(EffectAffectType.Target, ctx.Effect.Affects);
            reader.EndForEachIndex();
        }
    }

    public class WithAdditionnalContext : DotsTest
    {

        [Test]
        public void OnlyTriggerEffectOfActiveSkill([Values(AbilityState.CooledDown, AbilityState.CoolingDown, AbilityState.Casting)] AbilityState state)
        {
            // Arrange
            Entity caster = _entityManager.CreateEntity();

            _entityManager.AddComponentData(caster, new Target() { Value = Entity.Null });

            _entityManager.AddComponentData(caster, new TestResource() { Value = 20 });

            Ability ability = new Ability(1, 1, new Range())
            {
                State = AbilityState.Active
            };

            DynamicBuffer<AbilityBuffer> abilityBuffer = _entityManager.AddBuffer<AbilityBuffer>(caster);
            abilityBuffer.Add(new AbilityBuffer()
            {
                Ability = ability
            });
            abilityBuffer.Add(new AbilityBuffer()
            {
                Ability = new Ability(1, 1, new Range())
                {
                    State = state
                }
            });

            DynamicBuffer<TestEffectBuffer> testEffectBuffer = _entityManager.AddBuffer<TestEffectBuffer>(caster);
            testEffectBuffer.Add(new TestEffectBuffer()
            {
                AbilityIndex = 0,
                Effect = new TestEffect()
                {
                    Affects = EffectAffectType.Target,
                    Value = 1
                }
            });
            testEffectBuffer.Add(new TestEffectBuffer()
            {
                AbilityIndex = 1,
                Effect = new TestEffect()
                {
                    Affects = EffectAffectType.Target,
                    Value = 1
                }
            });

            _world.WithSystem<TestEffectTriggerSystem>();

            //Act
            _world.UpdateSystem<TestEffectTriggerSystem>();
            _world.CompleteAllJobs();

            //Assert
            TestEffectConsumerSystem conusmerSystem = _world.GetReference().GetExistingSystem<TestEffectConsumerSystem>();
            NativeStream.Reader reader = conusmerSystem.GetEffectReader();
            Assert.AreEqual(1, conusmerSystem.GetEffectReader().Count());
            reader.BeginForEachIndex(0);
            TestEffectContext ctx = reader.Read<TestEffectContext>();
            Assert.AreEqual(Entity.Null, ctx.Target);
            Assert.AreEqual(EffectAffectType.Target, ctx.Effect.Affects);
            Assert.AreEqual(1, ctx.Effect.Value);
            Assert.AreEqual(20, ctx.testResourceValue);
            reader.EndForEachIndex();
        }
    }
}
