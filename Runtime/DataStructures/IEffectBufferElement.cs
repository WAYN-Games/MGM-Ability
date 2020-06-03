using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    /// <summary>
    /// Interface to declare a new effect buffer to add to a runtime entity.
    /// </summary>
    /// <typeparam name="EFFECT"></typeparam>
    public interface IEffectBufferElement<EFFECT> : IBufferElementData where EFFECT : struct, IEffect
    {
        /// <summary>
        /// The index of the skill this effect is attached to.
        /// </summary>
        int SkillIndex { get; set; }
        /// <summary>
        /// The effect.
        /// </summary>
        EFFECT Effect { get; set; }
    }

}
