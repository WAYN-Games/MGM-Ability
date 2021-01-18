using System;

using Unity.Entities;

[Serializable]
public struct AbilityInput : IComponentData
{
    public uint AbilityId;
    public bool Enabled;

    public AbilityInput(uint abilityId) : this()
    {
        AbilityId = abilityId;
        Enabled = true;
    }
}
