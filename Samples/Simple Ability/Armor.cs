using Unity.Entities;

namespace WaynGroup.Mgm.Ability.Demo
{
    [GenerateAuthoringComponent]
    public struct Armor : IComponentData
    {
        public float Value;
    }
}
