using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    public enum EffectAffectType
    {
        Target,
        Self
    }

    /// <summary>
    /// Interface for declaring new effect struct.
    /// </summary>
    public interface IEffect : ISelfConvertingSkillComponentData
    {
        EffectAffectType Affects { get; set; }
    }

    public interface ISelfConvertingSkillComponentData : IComponentData
    {
        /// <summary>
        /// This method convert the effect and effect buffer entity and adds them to the entity.
        /// </summary>
        /// <param name="entity">Entity that can use the skill linked to this effect.</param>
        /// <param name="dstManager">Destination world entoty manager.</param>
        /// <param name="skillIndex">The index of the skill that the effect is linkde to.</param>
        void Convert(Entity entity, EntityManager dstManager, int skillIndex);
    }

    /// <summary>
    /// Interface for declaring new skill cost struct.
    /// </summary>
    public interface ISkillCost : ISelfConvertingSkillComponentData
    {
    }

    public interface ICostChecker<RESOURCE, COST> where RESOURCE : struct, IComponentData
        where COST : struct, ISkillCost
    {
        bool HasEnougthResourceLeft(COST cost, in RESOURCE resource);
    }

}
