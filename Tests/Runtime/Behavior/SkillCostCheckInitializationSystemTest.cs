using NUnit.Framework;

using Unity.Entities;

using WaynGroup.Mgm.Skill;
using WaynGroup.Mgm.Skill.Tests;

public class SkillCostCheckInitializationSystemTest : DotsTest
{
    [Test]
    public void ConsumeResourceWhenSkillEnabled()
    {
        // Arrange
        Entity caster = _entityManager.CreateEntity();

        Skill skill = new Skill(1, 1, new Range())
        {
            HasEnougthRessource = false,
            State = SkillState.Active
        };
        DynamicBuffer<SkillBuffer> skillBuffer = _entityManager.AddBuffer<SkillBuffer>(caster);
        skillBuffer.Add(new SkillBuffer()
        {
            Skill = skill
        });
        skillBuffer.Add(new SkillBuffer()
        {
            Skill = skill
        });

        _world.WithSystem<SkillCostCheckInitializationSystem>();

        //Act
        _world.UpdateSystem<SkillCostCheckInitializationSystem>();
        _world.CompleteAllJobs();

        //Assert
        Assert.IsTrue(_entityManager.GetBuffer<SkillBuffer>(caster)[0].Skill.HasEnougthRessource);
        Assert.IsTrue(_entityManager.GetBuffer<SkillBuffer>(caster)[1].Skill.HasEnougthRessource);

    }
}
