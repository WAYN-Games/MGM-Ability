using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{
    /// <summary>
    /// Interface for declaring new effect struct.
    /// </summary>
    public interface IEffect : IComponentData
    {
        /// <summary>
        /// This method convert the effect to and effect buffer entiry and adds itself to the entity.
        /// </summary>
        /// <param name="entity">Entity that can use the skill linked to this effect.</param>
        /// <param name="dstManager">Destination world entoty manager.</param>
        /// <param name="skillIndex">The index of the skill taht the effect is linkde to.</param>
        void Convert(Entity entity, EntityManager dstManager, int skillIndex);
    }

}
