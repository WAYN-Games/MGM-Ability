using WaynGroup.Mgm.Ability;

public struct EffectData
{
    #region Public Fields

    public uint Guid;
    public IEffect effect;

    #endregion Public Fields
}

public struct CostData
{
    #region Public Fields

    public uint Guid;
    public IAbilityCost cost;

    #endregion Public Fields
}