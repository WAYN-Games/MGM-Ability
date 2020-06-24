
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// Buffer of ability.
    /// Can be implcitly casted to and from Ability.
    /// </summary>
    public struct AbilityBuffer : IBufferElementData
    {
        public Ability Ability;

        public static implicit operator Ability(AbilityBuffer buffer) => buffer.Ability;
        public static implicit operator AbilityBuffer(Ability ability) => new AbilityBuffer() { Ability = ability };
    }

}
