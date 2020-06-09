using System;

namespace WaynGroup.Mgm.Skill
{
    /// <summary>
    /// The runtime representation of a skill.
    /// </summary>
    [Serializable]
    public struct Skill
    {
        public SkillState State { get; private set; }
        private Timing CoolDown;
        private Timing CastTime;
        public Range Range;
        public bool IsInRange;

        public Skill(float coolDown, float castTime, Range range) : this()
        {
            State = SkillState.CoolingDown;
            CoolDown = new Timing(coolDown);
            CastTime = new Timing(castTime);
            Range = range;
            IsInRange = false;
        }

        /// <summary>
        /// Mark the skill as inactive so that it's effects are no longer applied.
        /// </summary>
        public void Deactivate()
        {
            CoolDown.Reset();
            State = SkillState.CoolingDown;
        }

        /// <summary>
        /// Tries to start casting the skill.
        /// </summary>
        /// <returns>Returns a CastResult.</returns>
        public SkillCastResult TryCast()
        {
            if (State == SkillState.CoolingDown) return SkillCastResult.NotReady;
            if (!IsInRange) return SkillCastResult.OutOfRange;

            if (State != SkillState.Casting)
            {
                CastTime.Reset();
                State = SkillState.Casting;
                return SkillCastResult.Success;
            }

            return SkillCastResult.AlreadyCasting;
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
        /// Update the skill cast time and set the skill state to Active when the cast time is elapsed.
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
        /// Update the skill cooldowns and set the skill state to CooledDown when the cooldowns are elapsed.
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

}
