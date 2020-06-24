using NUnit.Framework;

using Unity.Entities;

using WaynGroup.Mgm.Skill;
using WaynGroup.Mgm.Skill.Tests;

public class SkillCostConsumerSystemTest : DotsTest
{
    [Test]
    public void ConsumeResourceWhenSkillEnabled()
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();

        Skill skill = new Skill(1, 1, new Range())
        {
            HasEnougthRessource = true,
            State = SkillState.Active
        };
        DynamicBuffer<SkillBuffer> skillBuffer = _entityManager.AddBuffer<SkillBuffer>(caster);
        skillBuffer.Add(new SkillBuffer()
        {
            Skill = skill
        });

        DynamicBuffer<TestCostBuffer> testCostBuffer = _entityManager.AddBuffer<TestCostBuffer>(caster);
        testCostBuffer.Add(new TestCostBuffer()
        {
            SkillIndex = 0,
            Cost = new TestCost() { Cost = 10 }
        }
        );

        DynamicBuffer<TestCost1Buffer> testCost1Buffer = _entityManager.AddBuffer<TestCost1Buffer>(caster);
        testCost1Buffer.Add(new TestCost1Buffer()
        {
            SkillIndex = 0,
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
    public void DoNotConsumeResourceWhenSkillIsNotEnabled([Values(SkillState.Casting, SkillState.CooledDown, SkillState.CoolingDown)]SkillState skillState)
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();

        Skill skill = new Skill(1, 1, new Range())
        {
            HasEnougthRessource = true,
            State = skillState
        };

        DynamicBuffer<SkillBuffer> skillBuffer = _entityManager.AddBuffer<SkillBuffer>(caster);
        skillBuffer.Add(new SkillBuffer()
        {
            Skill = skill
        });

        DynamicBuffer<TestCostBuffer> testCostBuffer = _entityManager.AddBuffer<TestCostBuffer>(caster);
        testCostBuffer.Add(new TestCostBuffer()
        {
            SkillIndex = 0,
            Cost = new TestCost() { Cost = 10 }
        }
        );

        DynamicBuffer<TestCost1Buffer> testCost1Buffer = _entityManager.AddBuffer<TestCost1Buffer>(caster);
        testCost1Buffer.Add(new TestCost1Buffer()
        {
            SkillIndex = 0,
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
