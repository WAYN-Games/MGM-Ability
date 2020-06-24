using NUnit.Framework;

using Unity.Entities;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Tests;

public class AbilityCostCheckInitializationSystemTest : DotsTest
{
    [Test]
    public void ConsumeResourceWhenAbilityEnabled()
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();

        Ability ability = new Ability(1, 1, new Range())
        {
            HasEnougthRessource = false,
            State = AbilityState.Active
        };
        DynamicBuffer<AbilityBuffer> abilityBuffer = _entityManager.AddBuffer<AbilityBuffer>(caster);
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = ability
        });
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = ability
        });

        _world.WithSystem<AbilityCostCheckInitializationSystem>();

        //Act
        _world.UpdateSystem<AbilityCostCheckInitializationSystem>();
        _world.CompleteAllJobs();

        //Assert
        Assert.IsTrue(_entityManager.GetBuffer<AbilityBuffer>(caster)[0].Ability.HasEnougthRessource);
        Assert.IsTrue(_entityManager.GetBuffer<AbilityBuffer>(caster)[1].Ability.HasEnougthRessource);

    }
}
