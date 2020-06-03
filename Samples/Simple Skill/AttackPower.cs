using Unity.Entities;

namespace WaynGroup.Mgm.Skill.Demo
{
    [GenerateAuthoringComponent]
    public struct AttackPower : IComponentData
    {
        public float Value;
    }
}
