using Unity.Entities;

namespace WaynGroup.Mgm.Skill.Demo
{
    [GenerateAuthoringComponent]
    public struct Armor : IComponentData
    {
        public float Value;
    }
}
