using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    [UpdateInGroup(typeof(AbilityUpdateSystemGroup))]
    public partial class CooldownRestrictionSystem : SystemBase
    {
        #region Protected Methods

        protected override void OnUpdate()
        {
            Entities.ForEach((ref AbilityInput abilityInput, in DynamicBuffer<AbilityCooldownBufferElement> cooldownBuffer,
                in AbilitiesMapIndex indexMap) =>
            {
                ref BlobMultiHashMap<uint, int> map = ref indexMap.guidToIndex.Value;
                var bufferIndex = map.GetValuesForKey(abilityInput.AbilityId)[0];
                if (cooldownBuffer[bufferIndex].CooldownTime > 0)
                    abilityInput.AddRestriction(16);
            }).WithBurst().ScheduleParallel();
        }

        #endregion Protected Methods
    }
}