using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    [GenerateAuthoringComponent]
    public struct Target : IComponentData
    {
        public Entity Value;
    }
}
