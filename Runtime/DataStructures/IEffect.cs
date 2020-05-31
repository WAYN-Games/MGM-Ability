using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    public interface IEffect : IComponentData
    {
        void Convert(Entity entity, EntityManager dstManager, int skillIndex);
    }

    public interface IPoolModifierEffect : IEffect
    {
        bool HasEnougthResources(float availableResourceAmount);
    }
    public interface IPoolComponentData : IComponentData
    {
        float Value { get; set; }
    }
}
