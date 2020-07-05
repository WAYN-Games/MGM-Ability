using System;

namespace WaynGroup.Mgm.Ability
{
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
        public float CurrentValue;



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

}
