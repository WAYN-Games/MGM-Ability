using NUnit.Framework;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Tests;

public class AbilityUpdateRangeSystemTest : DotsTest
{
    [Test]
    public void IsBelowMinRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 0) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0, 0, 0) });
        DynamicBuffer<AbilityBuffer> abilities = _entityManager.AddBuffer<AbilityBuffer>(attacker);
        abilities.Add(new Ability() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = true });


        _world.WithSystem<AbilityUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<AbilityUpdateRangeSystem>();
        _world.CompleteAllJobs();
        // Assert
        Assert.False(_entityManager.GetBuffer<AbilityBuffer>(attacker)[0].Ability.IsInRange);

    }

    [Test]
    public void IsAtMinRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 0.5f) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0, 0, 0) });
        DynamicBuffer<AbilityBuffer> abilities = _entityManager.AddBuffer<AbilityBuffer>(attacker);
        abilities.Add(new Ability() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = false });


        _world.WithSystem<AbilityUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<AbilityUpdateRangeSystem>();
        _world.CompleteAllJobs();
        // Assert
        Assert.True(_entityManager.GetBuffer<AbilityBuffer>(attacker)[0].Ability.IsInRange);

    }

    [Test]
    public void IsInRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 1) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0) });
        DynamicBuffer<AbilityBuffer> abilities = _entityManager.AddBuffer<AbilityBuffer>(attacker);
        abilities.Add(new Ability() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = false });


        _world.WithSystem<AbilityUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<AbilityUpdateRangeSystem>();
        _world.CompleteAllJobs();
        // Assert
        Assert.True(_entityManager.GetBuffer<AbilityBuffer>(attacker)[0].Ability.IsInRange);
    }

    [Test]
    public void IsAtMaxRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 10) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0) });
        DynamicBuffer<AbilityBuffer> abilities = _entityManager.AddBuffer<AbilityBuffer>(attacker);
        abilities.Add(new Ability() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = false });


        _world.WithSystem<AbilityUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<AbilityUpdateRangeSystem>();
        _world.CompleteAllJobs();
        // Assert
        Assert.True(_entityManager.GetBuffer<AbilityBuffer>(attacker)[0].Ability.IsInRange);
    }

    [Test]
    public void IsAboveMaxRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 11) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0) });
        DynamicBuffer<AbilityBuffer> abilities = _entityManager.AddBuffer<AbilityBuffer>(attacker);
        abilities.Add(new Ability() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = true });


        _world.WithSystem<AbilityUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<AbilityUpdateRangeSystem>();
        _world.CompleteAllJobs();
        // Assert
        Assert.False(_entityManager.GetBuffer<AbilityBuffer>(attacker)[0].Ability.IsInRange);
    }

}
