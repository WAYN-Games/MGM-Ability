using NUnit.Framework;

using Unity.Entities;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Tests;

public class AbilityEffectTriggerSystemTest : DotsTest
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

        Assert.AreEqual(1, conusmerSystem.GetEffectReader().ComputeItemCount());

    }
}
