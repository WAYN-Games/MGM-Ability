using System;

namespace WaynGroup.Mgm.Skill
{
    [Serializable]
    public enum SkillCastResult // May need to be replaced by a bit mask updated by one or several systems
    {
        Success,                // The skill started casting
        AlreadyCasting,         // The skill started casting
        NotReady,               // The skill is not fully cooled downed yet
        NotEnougthRessource,    // The player does not have enougth ressource to cast the skill ("not enougth mana") (not implemented and that is probably not the best place to do that... a dedicated system per ressource type would be better I think)
        OutOfRange              // The skill's target is too far away (not implemented and that is probably not the best place to do that... a dedicated system per targeting mode would be better I think)
    }

}
