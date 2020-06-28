using NUnit.Framework;

using Unity.Entities;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Tests;

public class AbilityCostConsumerSystemTest : DotsTest
{
    [Test]
    public void ConsumeResourceWhenAbilityEnabled()
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();

        Ability ability = new Ability(1, 1, new Range())
        {
            HasEnougthRessource = true,
            State = AbilityState.Active
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
            Cost = new TestCost1() { Cost = 5 }
        }
        );

        _entityManager.AddComponentData(caster, new TestResource() { Value = 20 });
        _entityManager.AddComponentData(caster, new TestResource1() { Value = 25 });

        _world.WithSystem<TestCostConsumerSystem>();
        _world.WithSystem<TestCost1ConsumerSystem>();

        //Act
        _world.UpdateSystem<TestCostConsumerSystem>();
        _world.UpdateSystem<TestCost1ConsumerSystem>();
        _world.CompleteAllJobs();

        //Assert
        Assert.AreEqual(10, _entityManager.GetComponentData<TestResource>(caster).Value);
        Assert.AreEqual(20, _entityManager.GetComponentData<TestResource1>(caster).Value);

    }

    [Test]
    public void DoNotConsumeResourceWhenAbilityIsNotEnabled([Values(AbilityState.Casting, AbilityState.CooledDown, AbilityState.CoolingDown)]AbilityState abilityState)
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();

        Ability ability = new Ability(1, 1, new Range())
        {
            HasEnougthRessource = true,
            State = abilityState
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
            Cost = new TestCost1() { Cost = 5 }
        }
        );

        _entityManager.AddComponentData(caster, new TestResource() { Value = 20 });
        _entityManager.AddComponentData(caster, new TestResource1() { Value = 25 });

        _world.WithSystem<TestCostConsumerSystem>();
        _world.WithSystem<TestCost1ConsumerSystem>();

        //Act
        _world.UpdateSystem<TestCostConsumerSystem>();
        _world.UpdateSystem<TestCost1ConsumerSystem>();
        _world.CompleteAllJobs();

        //Assert
        Assert.AreEqual(20, _entityManager.GetComponentData<TestResource>(caster).Value);
        Assert.AreEqual(25, _entityManager.GetComponentData<TestResource1>(caster).Value);
    }
}
