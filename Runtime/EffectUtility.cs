using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    public static class EffectUtility
    {
        public static void AddEffect<BUFFER, EFFECT>(Entity entity, EntityManager dstManager, int skillIndex, EFFECT effect) where EFFECT : struct, IEffect where BUFFER : struct, IEffectBufferElement<EFFECT>
        {
            DynamicBuffer<BUFFER> buffer = dstManager.HasComponent<BUFFER>(entity) ? dstManager.GetBuffer<BUFFER>(entity) : dstManager.AddBuffer<BUFFER>(entity);
            buffer.Add(new BUFFER()
            {
                SkillIndex = skillIndex,
                Effect = effect
            });
        }
    }

}
