using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    [GenerateAuthoringComponent]
    public struct Target : IComponentData
    {
        #region Public Fields

        public Entity Value;

        #endregion Public Fields
    }
}