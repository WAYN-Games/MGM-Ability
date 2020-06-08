
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using WaynGroup.Mgm.Skill;
using WaynGroup.Mgm.Skill.Tests;

public class SkillUpdateSystemTest : DotsTest
{
    [Test]
    public void SkillUpdateRangeSystem_IsBelowMinRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 0) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0, 0, 0) });
        DynamicBuffer<SkillBuffer> skills = _entityManager.AddBuffer<SkillBuffer>(attacker);
        skills.Add(new Skill() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = true });


        _world.WithSystem<SkillUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<SkillUpdateRangeSystem>();

        // Assert
        Assert.False(_entityManager.GetBuffer<SkillBuffer>(attacker)[0].Skill.IsInRange);

    }

    [Test]
    public void SkillUpdateRangeSystem_IsAtMinRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 0.5f) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0, 0, 0) });
        DynamicBuffer<SkillBuffer> skills = _entityManager.AddBuffer<SkillBuffer>(attacker);
        skills.Add(new Skill() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = false });


        _world.WithSystem<SkillUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<SkillUpdateRangeSystem>();

        // Assert
        Assert.True(_entityManager.GetBuffer<SkillBuffer>(attacker)[0].Skill.IsInRange);

    }

    [Test]
    public void SkillUpdateRangeSystem_IsInRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 1) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0) });
        DynamicBuffer<SkillBuffer> skills = _entityManager.AddBuffer<SkillBuffer>(attacker);
        skills.Add(new Skill() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = false });


        _world.WithSystem<SkillUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<SkillUpdateRangeSystem>();

        // Assert
        Assert.True(_entityManager.GetBuffer<SkillBuffer>(attacker)[0].Skill.IsInRange);
    }

    [Test]
    public void SkillUpdateRangeSystem_IsAtMaxRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 10) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0) });
        DynamicBuffer<SkillBuffer> skills = _entityManager.AddBuffer<SkillBuffer>(attacker);
        skills.Add(new Skill() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = false });


        _world.WithSystem<SkillUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<SkillUpdateRangeSystem>();

        // Assert
        Assert.True(_entityManager.GetBuffer<SkillBuffer>(attacker)[0].Skill.IsInRange);
    }

    [Test]
    public void SkillUpdateRangeSystem_IsAboveMaxRange()
    {
        // Arrange
        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Translation() { Value = new float3(0, 0, 11) });

        Entity attacker = _entityManager.CreateEntity();
        _entityManager.AddComponentData(attacker, new Target() { Value = target });
        _entityManager.AddComponentData(attacker, new Translation() { Value = new float3(0) });
        DynamicBuffer<SkillBuffer> skills = _entityManager.AddBuffer<SkillBuffer>(attacker);
        skills.Add(new Skill() { Range = new Range() { Min = 0.5f, Max = 10f }, IsInRange = true });


        _world.WithSystem<SkillUpdateRangeSystem>();

        // Act
        _world.UpdateSystem<SkillUpdateRangeSystem>();

        // Assert
        Assert.False(_entityManager.GetBuffer<SkillBuffer>(attacker)[0].Skill.IsInRange);
    }

}
