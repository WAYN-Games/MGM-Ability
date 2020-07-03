
using NUnit.Framework;

using Unity.Entities;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Tests;

public class AbilityUpdateTimingsSystemTest : DotsTest
{
    [Test]
    public void UpdateCastTimeOnlyWhenCasting([Values(AbilityState.Active, AbilityState.CooledDown, AbilityState.CoolingDown)]AbilityState state)
    {
        // Arrange

        Entity caster = _entityManager.CreateEntity();


        DynamicBuffer<AbilityBuffer> abilityBuffer = _entityManager.AddBuffer<AbilityBuffer>(caster);
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = new Ability(2, 2, new Range())
            {
                State = AbilityState.Casting
            }
        });
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = new Ability(1, 1, new Range())
            {
                State = state
            }
        });
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = new Ability(3, 3, new Range())
            {
                State = AbilityState.Casting
            }
        });
        _world.WithSystem<AbilityUpdateTimingsSystem>();
        _world.GetReference().SetTime(new Unity.Core.TimeData(0, 0.5f));

        // Act
        _world.UpdateSystem<AbilityUpdateTimingsSystem>();
        _world.CompleteAllJobs();

        // Assert
        Assert.AreEqual(1.5f, _entityManager.GetBuffer<AbilityBuffer>(caster)[0].Ability.CastTime.CurrentValue);
        Assert.AreEqual(1f, _entityManager.GetBuffer<AbilityBuffer>(caster)[1].Ability.CastTime.CurrentValue);
        Assert.AreEqual(2.5f, _entityManager.GetBuffer<AbilityBuffer>(caster)[2].Ability.CastTime.CurrentValue);
    }


    [Test]
    public void UpdateColldownTimeOnlyWhenCoolingdown([Values(AbilityState.Active, AbilityState.CooledDown, AbilityState.Casting)]AbilityState state)
    {
        // Arrange

        Entity caster = _entityManager.CreateEntity();


        DynamicBuffer<AbilityBuffer> abilityBuffer = _entityManager.AddBuffer<AbilityBuffer>(caster);
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = new Ability(2, 2, new Range())
            {
                State = AbilityState.CoolingDown
            }
        });
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = new Ability(1, 1, new Range())
            {
                State = state
            }
        });
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = new Ability(3, 3, new Range())
            {
                State = AbilityState.CoolingDown
            }
        });
        _world.WithSystem<AbilityUpdateTimingsSystem>();
        _world.GetReference().SetTime(new Unity.Core.TimeData(0, 0.5f));

        // Act
        _world.UpdateSystem<AbilityUpdateTimingsSystem>();
        _world.CompleteAllJobs();

        // Assert
        Assert.AreEqual(1.5, _entityManager.GetBuffer<AbilityBuffer>(caster)[0].Ability.CoolDown.CurrentValue);
        Assert.AreEqual(1, _entityManager.GetBuffer<AbilityBuffer>(caster)[1].Ability.CoolDown.CurrentValue);
        Assert.AreEqual(2.5, _entityManager.GetBuffer<AbilityBuffer>(caster)[2].Ability.CoolDown.CurrentValue);
    }

}
