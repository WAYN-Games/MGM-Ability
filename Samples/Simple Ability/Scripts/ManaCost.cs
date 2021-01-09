namespace WaynGroup.Mgm.Ability.Demo
{
    public struct ManaCost : IAbilityCost
    {
        public float Cost;

    }

    public struct ManaCostHandler : ICostHandler<ManaCost, Mana>
    {
        public void ConsumeCost(ManaCost cost, ref Mana resource)
        {
            resource.Value -= cost.Cost;
        }

        public bool HasEnougthResourceLeft(ManaCost cost, in Mana resource)
        {
            return resource.Value >= cost.Cost;
        }
    }

    public class ManaCostConsumerSystem : AbilityCostHanlderSystem<ManaCost, Mana, ManaCostHandler>
    {

    }

}
