using System;

namespace WaynGroup.Mgm.Skill
{
    [Serializable]
    public enum SkillState
    {
        CooledDown,     // Skill is ready to use
        Casting,        // Skill is charging up and can be interupted
        CoolingDown,    // Skill is cooling down
        Active          // Skill will apply it's effect during this tick
    }

}
