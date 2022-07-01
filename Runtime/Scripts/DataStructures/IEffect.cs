using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    public enum TargetingMode
    {
        Target,
        Self
    }

    public enum ActivationPhase
    {
        CastingStart,
        CastingEnd
    }

    /// <summary>
    /// Interface for declaring new effect struct.
    /// </summary>
    public interface IEffect : IComponentData
    {
        #region Public Properties

        TargetingMode Affects { get; set; }
        ActivationPhase Phase { get; set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Interface for declaring new ability cost struct.
    /// </summary>
    public interface IAbilityCost : IComponentData
    {
    }

    public interface ICostHandler<COST, RESOURCE> where RESOURCE : struct, IComponentData
    {
        #region Public Methods

        void ConsumeCost(COST cost, ref RESOURCE resource);

        bool HasEnougthResourceLeft(COST cost, in RESOURCE resource);

        #endregion Public Methods
    }
}