using Unity.Entities;

namespace WaynGroup.Mgm.Skill.Demo
{
    public struct ManaCost : ISkillCost
    {
        public float Cost;

        public void Convert(Entity entity, EntityManager dstManager, int skillIndex)
        {
            EffectUtility.AddCost<ManaCostBuffer, ManaCost>(entity, dstManager, skillIndex, this);
        }
    }

    public struct ManaCostBuffer : ISkillCostBufferElement<ManaCost>
    {
        public int SkillIndex { get; set; }
        public ManaCost Cost { get; set; }
    }

    public struct ManaCostChecker : ICostChecker<Mana, ManaCost>
    {
        public bool HasEnougthResourceLeft(ManaCost cost, in Mana resource)
        {
            return resource.Value >= cost.Cost;
        }
    }

    public struct ManaCostConusmer : ICostConsumer<ManaCost, Mana>
    {
        public void ConsumeCost(ManaCost cost, ref Mana resource)
        {
            resource.Value -= cost.Cost;
        }
    }

    public class ManaCostConsumerSystem : SkillCostConsumerSystem<ManaCost, ManaCostBuffer, Mana, ManaCostConusmer>
    {
        protected override ManaCostConusmer GetCostConsumer()
        {
            return new ManaCostConusmer();
        }
    }

    public class ManaCostCheckerSystem : SkillCostCheckerSystem<ManaCostBuffer, ManaCost, ManaCostChecker, Mana>
    {
        protected override ManaCostChecker GetCostChecker()
        {
            return new ManaCostChecker();
        }
    }

}
