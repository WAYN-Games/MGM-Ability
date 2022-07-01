using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    public struct CurrentlyCasting : ISystemStateComponentData
    {
        #region Public Fields

        public float castTime;
        public uint abilityGuid;

        #endregion Public Fields

        #region Public Properties

        public bool IsCasting => !float.NaN.Equals(castTime);

        #endregion Public Properties
    }
}