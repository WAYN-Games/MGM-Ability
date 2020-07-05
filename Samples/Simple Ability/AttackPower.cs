using Unity.Entities;

namespace WaynGroup.Mgm.Ability.Demo
{
    [GenerateAuthoringComponent]
    public struct AttackPower : IComponentData
    {
        public float Value;
    }
}
