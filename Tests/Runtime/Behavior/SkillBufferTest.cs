using NUnit.Framework;

using WaynGroup.Mgm.Ability;

public class AbilityBufferTest
{
    [Test]
    public void AbilityImplicitCastToBuffer()
    {
        // Arrange
        Ability ability = new Ability() { Range = new Range() { Max = 10, Min = 5 } };

        //Act
        AbilityBuffer abilityBuffer = ability;

        //Assert
        Assert.AreEqual(ability.Range, abilityBuffer.Ability.Range);

    }

    [Test]
    public void BufferImplicitCastToAbility()
    {

        // Arrange
        AbilityBuffer abilityBuffer = new AbilityBuffer
        {
            Ability = new Ability() { Range = new Range() { Max = 10, Min = 5 } }
        };

        //Act
        Ability ability = abilityBuffer;

        //Assert
        Assert.AreEqual(abilityBuffer.Ability.Range, ability.Range);

    }

}
