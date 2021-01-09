
using System;

using WaynGroup.Mgm.Ability;

public struct EffectData
{
    public Guid guid;
    public IEffect effect;
}

public struct CostData
{
    public Guid guid;
    public IAbilityCost cost;
}
