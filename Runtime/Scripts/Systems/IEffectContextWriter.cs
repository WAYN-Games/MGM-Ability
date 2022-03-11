using Unity.Entities;

namespace WaynGroup.Mgm.Ability
{
    public interface IEffectContextWriter<EFFECT_CTX>
        where EFFECT_CTX : struct, IEffectContext
    {
        #region Public Methods

        /// <summary>
        /// Method to cash the necessary component data array ([ReadOnly]) used to populate the effect context in the BuildEffectContext method.
        /// </summary>
        /// <param name="chunk"></param>
        void PrepareChunk(ArchetypeChunk chunk);

        EFFECT_CTX BuildEffectContext(int casterEntityIndex);

        #endregion Public Methods
    }
}