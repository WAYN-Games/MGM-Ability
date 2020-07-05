using System;

using NUnit.Framework;

using Unity.Collections;
using Unity.Entities;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Tests;

public class AbilityEffectConsumerSystemTest : DotsTest
{
    [Test]
    public void ConusmeAllEffectOnTargetEntity()
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();
        Entity Target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(Target, new TestResource() { Value = 100 });

        _world.WithSystem<TestEffectConsumerSystem>();
        TestEffectConsumerSystem consumerSystem = _world.GetReference().GetExistingSystem<TestEffectConsumerSystem>();
        NativeStream.Writer effectWriter = consumerSystem.CreateConsumerWriter(2);
        effectWriter.BeginForEachIndex(0);
        effectWriter.Write(new TestEffectContext() { Target = Target, Effect = new TestEffect() { Value = 10 } });
        effectWriter.Write(new TestEffectContext() { Target = Target, Effect = new TestEffect() { Value = 20 } });
        effectWriter.EndForEachIndex();
        effectWriter.BeginForEachIndex(1);
        effectWriter.Write(new TestEffectContext() { Target = Target, Effect = new TestEffect() { Value = 5 } });
        effectWriter.Write(new TestEffectContext() { Target = Target, Effect = new TestEffect() { Value = 15 } });
        effectWriter.EndForEachIndex();


        // Act
        _world.UpdateSystem<TestEffectConsumerSystem>();
        _world.CompleteAllJobs();

        // Assert
        Assert.AreEqual(50, _entityManager.GetComponentData<TestResource>(Target).Value);
    }

    [Test]
    public void DoNotFailIfNoStreamWereCreated()
    {
        // Arrange
        _world.WithSystem<TestEffectConsumerSystem>();

        try
        {
            _world.UpdateSystem<TestEffectConsumerSystem>();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}
