using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    [GenerateAuthoringComponent]
    public struct Target : IComponentData
    {
        public Entity Value;
    }

}
