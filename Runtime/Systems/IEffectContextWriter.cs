using Unity.Collections;
using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    public interface IEffectContextWriter<EFFECT> where EFFECT : struct, IEffect
    {
        /// <summary>
        /// Method to cash the necessary component data array ([ReadOnly]) used to populate the effect context in the WriteContextualizedEffect method.
        /// </summary>
        /// <param name="chunk"></param>
        void PrepareChunk(ArchetypeChunk chunk);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityIndex">The caster entity indext used to get the component data to populate the effect context.</param>
        /// <param name="consumerWriter">The NativeStream.Writer into which we write the contextualized effect.</param>
        /// <param name="effect">The effect to contextualize.</param>
        /// <param name="Target">The target entity affected by the effect.</param>
        void WriteContextualizedEffect(int entityIndex, ref NativeStream.Writer consumerWriter, EFFECT effect, Entity target);
    }
}
