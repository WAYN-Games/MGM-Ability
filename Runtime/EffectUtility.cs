using Unity.Entities;

namespace WaynGroup.Mgm.Skill
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
        /// <param name="skillIndex"></param>
        /// <param name="effect"></param>
        public static void AddEffect<BUFFER, EFFECT>(Entity entity, EntityManager dstManager, int skillIndex, EFFECT effect) where EFFECT : struct, IEffect where BUFFER : struct, IEffectBufferElement<EFFECT>
        {
            DynamicBuffer<BUFFER> buffer = dstManager.HasComponent<BUFFER>(entity) ? dstManager.GetBuffer<BUFFER>(entity) : dstManager.AddBuffer<BUFFER>(entity);
            buffer.Add(new BUFFER()
            {
                SkillIndex = skillIndex,
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
        /// <param name="skillIndex"></param>
        /// <param name="effect"></param>
        public static void AddCost<BUFFER, COST>(Entity entity, EntityManager dstManager, int skillIndex, COST cost) where COST : struct, ISkillCost where BUFFER : struct, ISkillCostBufferElement<COST>
        {
            DynamicBuffer<BUFFER> buffer = dstManager.HasComponent<BUFFER>(entity) ? dstManager.GetBuffer<BUFFER>(entity) : dstManager.AddBuffer<BUFFER>(entity);
            buffer.Add(new BUFFER()
            {
                SkillIndex = skillIndex,
                Cost = cost
            });
        }
    }

}
