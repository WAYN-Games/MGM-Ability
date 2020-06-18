using Unity.Entities;

namespace WaynGroup.Mgm.Skill.Demo
{
    [GenerateAuthoringComponent]
    public struct Mana : IComponentData
    {
        public float Value;
    }
}
