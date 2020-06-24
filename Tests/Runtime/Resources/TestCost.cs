using Unity.Entities;

namespace WaynGroup.Mgm.Ability.Tests
{
    public struct TestCost : IAbilityCost
    {
        public float Cost;

        public void Convert(Entity entity, EntityManager dstManager, int abilityIndex)
        {
            EffectUtility.AddCost<TestCostBuffer, TestCost>(entity, dstManager, abilityIndex, this);
        }
    }

    public struct TestCostBuffer : IAbilityCostBufferElement<TestCost>
    {
        public int AbilityIndex { get; set; }
        public TestCost Cost { get; set; }
    }

    public struct TestResource : IComponentData
    {
        public float Value;
    }

    public struct TestCostChecker : ICostChecker<TestResource, TestCost>
    {
        public bool HasEnougthResourceLeft(TestCost cost, in TestResource resource)
        {
            return resource.Value >= cost.Cost;
        }
    }

    public struct TestCostConusmer : ICostConsumer<TestCost, TestResource>
    {
        public void ConsumeCost(TestCost cost, ref TestResource resource)
        {
            resource.Value -= cost.Cost;
        }
    }

    public class TestCostConsumerSystem : AbilityCostConsumerSystem<TestCost, TestCostBuffer, TestResource, TestCostConusmer>
    {
        protected override TestCostConusmer GetCostConsumer()
        {
            return new TestCostConusmer();
        }
    }

    public class TestCostCheckerSystem : AbilityCostCheckerSystem<TestCostBuffer, TestCost, TestCostChecker, TestResource>
    {
        protected override TestCostChecker GetCostChecker()
        {
            return new TestCostChecker();
        }
    }

}
