using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    public interface IEffectBufferElement<EFFECT> : IBufferElementData where EFFECT : struct, IEffect
    {
        int SkillIndex { get; set; }
        EFFECT Effect { get; set; }
    }

}
