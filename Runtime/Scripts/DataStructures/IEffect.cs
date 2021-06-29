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

    /// <summary>
    /// Interface for declaring new ability cost struct.
    /// </summary>
    public interface IAbilityCost : IComponentData
    {

    }


    public interface ICostHandler<COST, RESOURCE> where RESOURCE : struct, IComponentData
    {
        void ConsumeCost(COST cost, ref RESOURCE resource);
        bool HasEnougthResourceLeft(COST cost, in RESOURCE resource);
    }

}
