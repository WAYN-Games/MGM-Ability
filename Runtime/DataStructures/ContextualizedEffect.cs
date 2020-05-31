using Unity.Entities;

namespace WaynGroup.Mgm.Skill
{

    public struct ContextualizedEffect<EFFECT> where EFFECT : struct, IEffect
    {
        public Entity Target;
        public EFFECT Effect;
    }

}
