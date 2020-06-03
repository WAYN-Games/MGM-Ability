
using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    /// <summary>
    /// Buffer of skill.
    /// Can be implcitly casted to and from Skill.
    /// </summary>
    public struct SkillBuffer : IBufferElementData
    {
        public Skill Skill;

        public static implicit operator Skill(SkillBuffer buffer) => buffer.Skill;
        public static implicit operator SkillBuffer(Skill skill) => new SkillBuffer() { Skill = skill };
    }

}
