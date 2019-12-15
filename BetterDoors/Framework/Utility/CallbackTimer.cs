using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterDoors.Framework.Utility
{
    /// <summary> A timer that invokes callback functions at a regular interval.</summary>
    internal class CallbackTimer
    {
        /*********
        ** Fields
        *********/

        /// <summary>Map of current active callbacks => time left before invocation.</summary>
        private readonly IDictionary<Func<int>, int> activeCallbacks = new Dictionary<Func<int>, int>();

        /*********
        ** Public methods
        *********/

        /// <summary>Registers a callback function.</summary>
        /// <param name="callback">The function to call.</param>
        /// <param name="ms">How many ms before it should be invoked.</param>
        public void RegisterCallback(Func<int> callback, int ms)
        {
            this.activeCallbacks[callback] = ms;

            if (ms == 0)
                this.TimeElapsed(0);
        }

        /// <summary>Checks whether a callback is already registered.</summary>
        /// <param name="callback">The callback to check.</param>
        /// <returns>Whether the callback is already registered.</returns>
        public bool IsRegistered(Func<int> callback)
        {
            return this.activeCallbacks.ContainsKey(callback);
        }

        /// <summary>Handles time elapsing and invokes callback if necessary.</summary>
        /// <param name="ms">Milliseconds that have elapsed since the last call.</param>
        public void TimeElapsed(int ms)
        {
            // Process and call callbacks.
            IDictionary<Func<int>, int> callbacksToRemove = new Dictionary<Func<int>, int>();
            foreach (Func<int> callback in this.activeCallbacks.Keys.ToList())
            {
                this.activeCallbacks[callback] -= ms;

                if (this.activeCallbacks[callback] <= 0)
                {
                    callbacksToRemove[callback] = callback();
                }
            }

            // Reschedule callbacks based on return value of callback function.
            foreach (KeyValuePair<Func<int>, int> result in callbacksToRemove)
                if (result.Value < 0)
                    this.activeCallbacks.Remove(result.Key);
                else
                    this.activeCallbacks[result.Key] = result.Value;
        }
    }
}
