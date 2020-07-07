using Unity.Entities;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Demo;

namespace Namespace
{
    public struct NewCost : IAbilityCost
    {
        public float Cost;

        public void Convert(Entity entity, EntityManager dstManager, int abilityIndex)
        {
            EffectUtility.AddCost<NewCostBuffer, NewCost>(entity, dstManager, abilityIndex, this);
        }
    }

    public struct NewCostBuffer : IAbilityCostBufferElement<NewCost>
    {
        public int AbilityIndex { get; set; }
        public NewCost Cost { get; set; }
    }

    public struct NewCostChecker : ICostChecker<Mana, NewCost>
    {
        public bool HasEnougthResourceLeft(NewCost cost, in Mana resource)
        {
            return resource.Value >= cost.Cost;
        }
    }

    public struct NewCostConusmer : ICostConsumer<NewCost, Mana>
    {
        public void ConsumeCost(NewCost cost, ref Mana resource)
        {
            resource.Value -= cost.Cost;
        }
    }

    public class NewCostConsumerSystem : AbilityCostConsumerSystem<NewCost, NewCostBuffer, Mana, NewCostConusmer>
    {
        protected override NewCostConusmer GetCostConsumer()
        {
            return new NewCostConusmer();
        }
    }

    public class NewCostCheckerSystem : AbilityCostCheckerSystem<NewCostBuffer, NewCost, NewCostChecker, Mana>
    {
        protected override NewCostChecker GetCostChecker()
        {
            return new NewCostChecker();
        }
    }

}
