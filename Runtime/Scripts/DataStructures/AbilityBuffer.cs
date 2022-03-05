
using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// Buffer of ability references.
    /// </summary>
    [InternalBufferCapacity(0)] // Force the ability buffer to be stord out of chunk
    public struct AbilityCooldownBufferElement : IBufferElementData
    {
        public float CooldownTime;
    }

}
