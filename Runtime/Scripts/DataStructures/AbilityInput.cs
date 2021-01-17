using System;
using Unity.Entities;

[Serializable]
public struct AbilityInput : IComponentData
{
    public uint AbilityId;
}
