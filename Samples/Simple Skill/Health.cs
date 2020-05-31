using Unity.Entities;

namespace WaynGroup.Mgm.Skill.Demo
{
    [GenerateAuthoringComponent]
    public struct Health : IComponentData
    {
        public float Value;
    }
}
