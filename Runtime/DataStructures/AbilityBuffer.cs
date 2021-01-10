using System;

using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// Buffer of ability references.
    /// </summary>
    public struct AbilityBufferElement : IBufferElementData
    {
        /// <summary>
        /// Unique reference to the addressable asset of the ability.
        /// </summary>
        public Guid Guid;
        /// <summary>
        /// Current timing of the ability. Can either represent the CoolDown of Cast time.
        /// </summary>
        public float CurrentTimming;
        /// <summary>
        /// Current state of the ability.
        /// </summary>
        public AbilityState AbilityState;

        public bool HasEnougthRessource;

        public bool IsInRange;
    }

}
