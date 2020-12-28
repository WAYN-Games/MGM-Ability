using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    public enum TargetingMode
    {

        Target,
        Self

    }

    /// <summary>
    /// Interface for declaring new effect struct.
    /// </summary>
    public interface IEffect
    {
        TargetingMode Affects { get; set; }
    }

    public interface ISelfConvertingAbilityComponentData : IComponentData
    {
        /// <summary>
        /// This method convert the effect and effect buffer entity and adds them to the entity.
        /// </summary>
        /// <param name="entity">Entity that can use the ability linked to this effect.</param>
        /// <param name="dstManager">Destination world entoty manager.</param>
        /// <param name="abilityIndex">The index of the ability that the effect is linkde to.</param>
        void Convert(Entity entity, EntityManager dstManager, int abilityIndex);


    }

    /// <summary>
    /// Interface for declaring new ability cost struct.
    /// </summary>
    public interface IAbilityCost : ISelfConvertingAbilityComponentData
    {
    }

    public interface ICostChecker<RESOURCE, COST> where RESOURCE : struct, IComponentData
        where COST : struct, IAbilityCost
    {
        bool HasEnougthResourceLeft(COST cost, in RESOURCE resource);
    }

}
