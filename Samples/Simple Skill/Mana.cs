using Unity.Entities;

namespace WaynGroup.Mgm.Ability.Demo
{
    [GenerateAuthoringComponent]
    public struct Mana : IComponentData
    {
        public float Value;
    }
}
