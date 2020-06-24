using Unity.Entities;

using WaynGroup.Mgm.Skill.Demo;

namespace WaynGroup.Mgm.Skill.Tests
{
    public struct TestCost : ISkillCost
    {
        public float Cost;

        public void Convert(Entity entity, EntityManager dstManager, int skillIndex)
        {
            EffectUtility.AddCost<TestCostBuffer, TestCost>(entity, dstManager, skillIndex, this);
        }
    }

    public struct TestCostBuffer : ISkillCostBufferElement<TestCost>
    {
        public int SkillIndex { get; set; }
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

    public class TestCostConsumerSystem : SkillCostConsumerSystem<TestCost, TestCostBuffer, TestResource, TestCostConusmer>
    {
        protected override TestCostConusmer GetCostConsumer()
        {
            return new TestCostConusmer();
        }
    }

    public class TestCostCheckerSystem : SkillCostCheckerSystem<TestCostBuffer, TestCost, TestCostChecker, TestResource>
    {
        protected override TestCostChecker GetCostChecker()
        {
            return new TestCostChecker();
        }
    }

}
