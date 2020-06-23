using NUnit.Framework;

using WaynGroup.Mgm.Skill;

public class SkillBufferTest
{
    [Test]
    public void SkillImplicitCastToBuffer()
    {
        // Arrange
        Skill skill = new Skill() { Range = new Range() { Max = 10, Min = 5 } };

        //Act
        SkillBuffer skillBuffer = skill;

        //Assert
        Assert.AreEqual(skill.Range, skillBuffer.Skill.Range);

    }

    [Test]
    public void BufferImplicitCastToSkill()
    {

        // Arrange
        SkillBuffer skillBuffer = new SkillBuffer
        {
            Skill = new Skill() { Range = new Range() { Max = 10, Min = 5 } }
        };

        //Act
        Skill skill = skillBuffer;

        //Assert
        Assert.AreEqual(skillBuffer.Skill.Range, skill.Range);

    }

}
