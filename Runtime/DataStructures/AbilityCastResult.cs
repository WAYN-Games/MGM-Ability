using System;

namespace WaynGroup.Mgm.Ability
{
    [Serializable]
    public enum AbilityCastResult // May need to be replaced by a bit mask updated by one or several systems
    {
        Success,                // The ability started casting
        AlreadyCasting,         // The ability started casting
        NotReady,               // The ability is not fully cooled downed yet
        NotEnougthRessource,    // The player does not have enougth ressource to cast the ability ("not enougth mana") (not implemented and that is probably not the best place to do that... a dedicated system per ressource type would be better I think)
        OutOfRange              // The ability's target is too far away (not implemented and that is probably not the best place to do that... a dedicated system per targeting mode would be better I think)
    }

}
