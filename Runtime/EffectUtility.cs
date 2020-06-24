using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    public static class EffectUtility
    {
        /// <summary>
        /// This method takes care of adding the buffer and effect to the entity.
        /// </summary>
        /// <typeparam name="BUFFER"></typeparam>
        /// <typeparam name="EFFECT"></typeparam>
        /// <param name="entity"></param>
        /// <param name="dstManager"></param>
        /// <param name="abilityIndex"></param>
        /// <param name="effect"></param>
        public static void AddEffect<BUFFER, EFFECT>(Entity entity, EntityManager dstManager, int abilityIndex, EFFECT effect) where EFFECT : struct, IEffect where BUFFER : struct, IEffectBufferElement<EFFECT>
        {
            DynamicBuffer<BUFFER> buffer = dstManager.HasComponent<BUFFER>(entity) ? dstManager.GetBuffer<BUFFER>(entity) : dstManager.AddBuffer<BUFFER>(entity);
            buffer.Add(new BUFFER()
            {
                AbilityIndex = abilityIndex,
                Effect = effect
            });
        }

        /// <summary>
        /// This method takes care of adding the buffer and effect to the entity.
        /// </summary>
        /// <typeparam name="BUFFER"></typeparam>
        /// <typeparam name="EFFECT"></typeparam>
        /// <param name="entity"></param>
        /// <param name="dstManager"></param>
        /// <param name="abilityIndex"></param>
        /// <param name="effect"></param>
        public static void AddCost<BUFFER, COST>(Entity entity, EntityManager dstManager, int abilityIndex, COST cost) where COST : struct, IAbilityCost where BUFFER : struct, IAbilityCostBufferElement<COST>
        {
            DynamicBuffer<BUFFER> buffer = dstManager.HasComponent<BUFFER>(entity) ? dstManager.GetBuffer<BUFFER>(entity) : dstManager.AddBuffer<BUFFER>(entity);
            buffer.Add(new BUFFER()
            {
                AbilityIndex = abilityIndex,
                Cost = cost
            });
        }
    }

}
