using System;

namespace WaynGroup.Mgm.Ability
{
    /// <summary>
    /// The runtime representation of a ability.
    /// </summary>
    [Serializable]
    public struct Ability
    {
        public Guid Guid;
        public AbilityState State { get; set; }
        public Timing CoolDown;
        public Timing CastTime;
        public Range Range;
        public bool IsInRange;
        public bool HasEnougthRessource;

        public Ability(float coolDown, float castTime, Range range) : this()
        {
            State = AbilityState.CoolingDown;
            CoolDown = new Timing(coolDown);
            CastTime = new Timing(castTime);
            Range = range;
            IsInRange = false;
            HasEnougthRessource = true;
        }

        /// <summary>
        /// Mark the ability as inactive so that it's effects are no longer applied.
        /// </summary>
        public void StartCooloingDown()
        {
            CoolDown.Reset();
            State = AbilityState.CoolingDown;
        }

        /// <summary>
        /// Tries to start casting the ability.
        /// </summary>
        /// <returns>Returns a CastResult.</returns>
        public AbilityCastResult TryCast()
        {
            if (State == AbilityState.CoolingDown) return AbilityCastResult.NotReady;
            if (!IsInRange) return AbilityCastResult.OutOfRange;
            if (!HasEnougthRessource) return AbilityCastResult.NotEnougthRessource;

            if (State != AbilityState.Casting)
            {
                CastTime.Reset();
                State = AbilityState.Casting;
                return AbilityCastResult.Success;
            }

            return AbilityCastResult.AlreadyCasting;
        }

        /// <summary>
        /// Update the ability cast time and set the ability state to Active when the cast time is elapsed.
        /// </summary>
        /// <param name="deltaTime">The amount of time elapsed since the last update.</param>
        public void UpdateCastTime(float deltaTime)
        {
            CastTime.Update(deltaTime);
            if (CastTime.IsElapsed())
            {
                State = AbilityState.Active;
            }
        }

        /// <summary>
        /// Update the ability cooldowns and set the ability state to CooledDown when the cooldowns are elapsed.
        /// </summary>
        /// <param name="deltaTime">The amount of time elapsed since the last update.</param>
        public void UpdateCoolDowns(float deltaTime)
        {
            CoolDown.Update(deltaTime);
            if (CoolDown.IsElapsed())
            {
                State = AbilityState.CooledDown;
            }
        }
    }

}
