using Unity.Entities;

namespace WaynGroup.Mgm.Ability.Demo
{
    [GenerateAuthoringComponent]
    public struct Health : IComponentData
    {
        public float Value;
    }
}
