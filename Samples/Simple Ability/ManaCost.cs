using Unity.Entities;

namespace WaynGroup.Mgm.Ability.Demo
{
    public struct ManaCost : IAbilityCost
    {
        public float Cost;

        public void Convert(Entity entity, EntityManager dstManager, int abilityIndex)
        {
            EffectUtility.AddCost<ManaCostBuffer, ManaCost>(entity, dstManager, abilityIndex, this);
        }
    }

    public struct ManaCostBuffer : IAbilityCostBufferElement<ManaCost>
    {
        public int AbilityIndex { get; set; }
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

    public class ManaCostConsumerSystem : AbilityCostConsumerSystem<ManaCost, ManaCostBuffer, Mana, ManaCostConusmer>
    {
        protected override ManaCostConusmer GetCostConsumer()
        {
            return new ManaCostConusmer();
        }
    }

    public class ManaCostCheckerSystem : AbilityCostCheckerSystem<ManaCostBuffer, ManaCost, ManaCostChecker, Mana>
    {
        protected override ManaCostChecker GetCostChecker()
        {
            return new ManaCostChecker();
        }
    }

}
