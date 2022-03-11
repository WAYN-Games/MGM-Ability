using System;

namespace WaynGroup.Mgm.Ability
{
    [Serializable]
    public enum AbilityState
    {
        CooledDown,     // Ability is ready to use
        Casting,        // Ability is charging up and can be interupted
        CoolingDown,    // Ability is cooling down
        Active          // Ability will apply it's effect during this tick
    }
}