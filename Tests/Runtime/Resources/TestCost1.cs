using Unity.Entities;

namespace WaynGroup.Mgm.Ability.Tests
{
    public struct TestCost1 : IAbilityCost
    {
        public float Cost;

        public void Convert(Entity entity, EntityManager dstManager, int abilityIndex)
        {
            EffectUtility.AddCost<TestCost1Buffer, TestCost1>(entity, dstManager, abilityIndex, this);
        }
    }

    public struct TestCost1Buffer : IAbilityCostBufferElement<TestCost1>
    {
        public int AbilityIndex { get; set; }
        public TestCost1 Cost { get; set; }
    }

    public struct TestResource1 : IComponentData
    {
        public float Value;
    }

    public struct TestCost1Checker : ICostChecker<TestResource1, TestCost1>
    {
        public bool HasEnougthResourceLeft(TestCost1 cost, in TestResource1 resource)
        {

            return resource.Value >= cost.Cost;
        }
    }

    public struct TestCost1Conusmer : ICostConsumer<TestCost1, TestResource1>
    {
        public void ConsumeCost(TestCost1 cost, ref TestResource1 resource)
        {
            resource.Value -= cost.Cost;
        }
    }

    public class TestCost1ConsumerSystem : AbilityCostConsumerSystem<TestCost1, TestCost1Buffer, TestResource1, TestCost1Conusmer>
    {
        protected override TestCost1Conusmer GetCostConsumer()
        {
            return new TestCost1Conusmer();
        }
    }

    public class TestCost1CheckerSystem : AbilityCostCheckerSystem<TestCost1Buffer, TestCost1, TestCost1Checker, TestResource1>
    {
        protected override TestCost1Checker GetCostChecker()
        {
            return new TestCost1Checker();
        }
    }

}
