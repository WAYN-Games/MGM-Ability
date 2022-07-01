using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class AbilitySystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(AbilitySystemGroup))]
    public class AbilityUpdateSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateAfter(typeof(AbilityUpdateSystemGroup))]
    [UpdateInGroup(typeof(AbilitySystemGroup))]
    public class AbilityTriggerSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateAfter(typeof(AbilityUpdateSystemGroup))]
    [UpdateInGroup(typeof(AbilitySystemGroup))]
    public class AbilityCostsSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(AbilityCostsSystemGroup))]
    public class AbilityCostsCheckerSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(AbilityCostsSystemGroup))]
    [UpdateAfter(typeof(AbilityCostsCheckerSystemGroup))]
    public class AbilityCostsConsumerSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateAfter(typeof(AbilityTriggerSystemGroup))]
    [UpdateInGroup(typeof(AbilitySystemGroup))]
    public class AbilityConsumerSystemGroup : ComponentSystemGroup
    {
    }
}