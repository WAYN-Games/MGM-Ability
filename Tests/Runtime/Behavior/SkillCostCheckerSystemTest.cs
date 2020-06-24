using NUnit.Framework;

using Unity.Entities;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Tests;

public class AbilityCostCheckerSystemTest : DotsTest
{
    [Test]
    public void DoesNotHaveEnougthOfAllResources()
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();

        Ability ability = new Ability(1, 1, new Range())
        {
            HasEnougthRessource = true
        };

        DynamicBuffer<AbilityBuffer> abilityBuffer = _entityManager.AddBuffer<AbilityBuffer>(caster);
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = ability
        });

        DynamicBuffer<TestCostBuffer> testCostBuffer = _entityManager.AddBuffer<TestCostBuffer>(caster);
        testCostBuffer.Add(new TestCostBuffer()
        {
            AbilityIndex = 0,
            Cost = new TestCost() { Cost = 10 }
        }
        );

        DynamicBuffer<TestCost1Buffer> testCost1Buffer = _entityManager.AddBuffer<TestCost1Buffer>(caster);
        testCost1Buffer.Add(new TestCost1Buffer()
        {
            AbilityIndex = 0,
            Cost = new TestCost1() { Cost = 10 }
        }
        );

        _entityManager.AddComponentData(caster, new TestResource() { Value = 0 });
        _entityManager.AddComponentData(caster, new TestResource1() { Value = 0 });

        _world.WithSystem<TestCostCheckerSystem>();
        _world.WithSystem<TestCost1CheckerSystem>();

        //Act
        _world.UpdateSystem<TestCostCheckerSystem>();
        _world.UpdateSystem<TestCost1CheckerSystem>();
        _world.CompleteAllJobs();

        //Assert
        Assert.IsFalse(_entityManager.GetBuffer<AbilityBuffer>(caster)[0].Ability.HasEnougthRessource);

    }

    [Test]
    public void HasEnougthOfAllResources()
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();

        Ability ability = new Ability(1, 1, new Range())
        {
            HasEnougthRessource = true
        };

        DynamicBuffer<AbilityBuffer> abilityBuffer = _entityManager.AddBuffer<AbilityBuffer>(caster);
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = ability
        });

        DynamicBuffer<TestCostBuffer> testCostBuffer = _entityManager.AddBuffer<TestCostBuffer>(caster);
        testCostBuffer.Add(new TestCostBuffer()
        {
            AbilityIndex = 0,
            Cost = new TestCost() { Cost = 10 }
        }
        );

        DynamicBuffer<TestCost1Buffer> testCost1Buffer = _entityManager.AddBuffer<TestCost1Buffer>(caster);
        testCost1Buffer.Add(new TestCost1Buffer()
        {
            AbilityIndex = 0,
            Cost = new TestCost1() { Cost = 10 }
        }
        );

        _entityManager.AddComponentData(caster, new TestResource() { Value = 100 });
        _entityManager.AddComponentData(caster, new TestResource1() { Value = 100 });

        _world.WithSystem<TestCostCheckerSystem>();
        _world.WithSystem<TestCost1CheckerSystem>();

        //Act
        _world.UpdateSystem<TestCostCheckerSystem>();
        _world.UpdateSystem<TestCost1CheckerSystem>();
        _world.CompleteAllJobs();

        //Assert
        Assert.IsTrue(_entityManager.GetBuffer<AbilityBuffer>(caster)[0].Ability.HasEnougthRessource);
    }

    [Test]
    public void DoesNotHaveEnougthOfAtLeastOneOfTheResources()
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();

        Ability ability = new Ability(1, 1, new Range())
        {
            HasEnougthRessource = true
        };

        DynamicBuffer<AbilityBuffer> abilityBuffer = _entityManager.AddBuffer<AbilityBuffer>(caster);
        abilityBuffer.Add(new AbilityBuffer()
        {
            Ability = ability
        });

        DynamicBuffer<TestCostBuffer> testCostBuffer = _entityManager.AddBuffer<TestCostBuffer>(caster);
        testCostBuffer.Add(new TestCostBuffer()
        {
            AbilityIndex = 0,
            Cost = new TestCost() { Cost = 10 }
        }
        );

        DynamicBuffer<TestCost1Buffer> testCost1Buffer = _entityManager.AddBuffer<TestCost1Buffer>(caster);
        testCost1Buffer.Add(new TestCost1Buffer()
        {
            AbilityIndex = 0,
            Cost = new TestCost1() { Cost = 10 }
        }
        );

        _entityManager.AddComponentData(caster, new TestResource() { Value = 100 });
        _entityManager.AddComponentData(caster, new TestResource1() { Value = 0 });

        _world.WithSystem<TestCostCheckerSystem>();
        _world.WithSystem<TestCost1CheckerSystem>();

        //Act
        _world.UpdateSystem<TestCostCheckerSystem>();
        _world.UpdateSystem<TestCost1CheckerSystem>();
        _world.CompleteAllJobs();

        //Assert
        Assert.IsFalse(_entityManager.GetBuffer<AbilityBuffer>(caster)[0].Ability.HasEnougthRessource);
    }

}
