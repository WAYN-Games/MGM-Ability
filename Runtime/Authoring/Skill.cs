using System;

using Unity.Entities;

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

    [Serializable]
    public enum CastResult // May need to be replaced by a bit mask
    {
        Success,                // The skill started casting
        AlreadyCasting,                // The skill started casting
        NotReady,               // The skill is not fully cooled downed yet
        NotEnougthRessource,    // The player does not have enougth ressource to cast the skill ("not enougth mana") (not implemented and that is probably not the best place to do that... a dedicated system per ressource type would be better I think)
        OutOfRange              // The skill's target is too far away (not implemented and that is probably not the best place to do that... a dedicated system per targeting mode would be better I think)
    }

    [Serializable]
    public struct Skill
    {
        public SkillState State { get; private set; }
        private Timing CoolDown;
        private Timing CastTime;
        public int Index;

        public Skill(float coolDown, float castTime, int index) : this()
        {
            State = SkillState.CooledDown;
            CoolDown = new Timing(coolDown);
            CastTime = new Timing(castTime);
            Index = index;
        }

        /// <summary>
        /// Mark the skill as inactive so that it's effects are no longer applied.
        /// </summary>
        public void Deactivate()
        {
            CoolDown.Reset();
            State = SkillState.CoolingDown;
        }


        public CastResult TryCast()
        {
            if (State == SkillState.CoolingDown) return CastResult.NotReady;

            if (State != SkillState.Casting)
            {
                CastTime.Reset();
                State = SkillState.Casting;
                return CastResult.Success;
            }

            return CastResult.AlreadyCasting;
        }

        /// <summary>
        /// Determine if the skill effects should be applied.
        /// </summary>
        /// <returns>True if the skill effects should be aplied, false otherwise.</returns>
        public bool ShouldApplyEffects()
        {
            return State == SkillState.Active;
        }

        /// <summary>
        /// Update the skill cast time.
        /// </summary>
        /// <param name="deltaTime">The amount of time elapsed since the last update.</param>
        public void UpdateCastTime(float deltaTime)
        {
            CastTime.Update(deltaTime);
            if (CastTime.IsElapsed())
            {
                State = SkillState.Active;
            }
        }

        /// <summary>
        /// Update the skill cooldowns.
        /// </summary>
        /// <param name="deltaTime">The amount of time elapsed since the last update.</param>
        public void UpdateCoolDowns(float deltaTime)
        {
            CoolDown.Update(deltaTime);
            if (CoolDown.IsElapsed())
            {
                State = SkillState.CooledDown;
            }
        }
    }

    /// <summary>
    /// Represents a timeframe.
    /// </summary>
    [Serializable]
    public struct Timing
    {
        /// <summary>
        /// The original amount of time on the timing.
        /// </summary>
        private readonly float OrginalValue;
        /// <summary>
        /// The current remaining time on the timing.
        /// </summary>
        private float CurrentValue;



        /// <summary>
        /// Initialize the timing to the number of seconds passed as value.
        /// </summary>
        /// <param name="value">The number of seconds for that timing.</param>
        public Timing(float value)
        {
            CurrentValue = value;
            OrginalValue = value;
        }

        /// <summary>
        /// Determines if the timing has elapsed.
        /// </summary>
        /// <returns>True if there is no remaining time on the timing, false otherwise.</returns>
        public bool IsElapsed()
        {
            return CurrentValue <= 0;
        }

        /// <summary>
        /// Update the remaining time on the timing.
        /// </summary>
        /// <param name="deltaTime">The amount of time to subtract from the remaining time.</param>
        public void Update(float deltaTime)
        {
            CurrentValue -= deltaTime;
        }

        /// <summary>
        /// Resert the timing to it's original value.
        /// </summary>
        public void Reset()
        {
            CurrentValue = OrginalValue;
        }
    }

    /// <summary>
    /// Buffer of skill.
    /// Can be implcitly casted to and from Skill.
    /// </summary>
    public struct SkillBuffer : IBufferElementData
    {
        public Skill Skill;

        public static implicit operator Skill(SkillBuffer buffer) => buffer.Skill;
        public static implicit operator SkillBuffer(Skill skill) => new SkillBuffer() { Skill = skill };
    }

}
