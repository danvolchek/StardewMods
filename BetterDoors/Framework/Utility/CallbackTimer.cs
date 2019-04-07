using System.Collections.Generic;
using System.Linq;

namespace BetterDoors.Framework.Utility
{
    /// <summary>
    /// A timer that invokes callback functions at a regular interval.
    /// </summary>
    internal class CallbackTimer
    {
        private readonly IDictionary<Callback, int> activeCallbacks = new Dictionary<Callback, int>();

        public void RegisterCallback(Callback callback, int ms)
        {
            this.activeCallbacks[callback] = ms;

            if (ms == 0)
                this.TimeElapsed(0);
        }

        public bool IsRegistered(Callback callback)
        {
            return this.activeCallbacks.ContainsKey(callback);
        }

        public void TimeElapsed(int ms)
        {
            IDictionary<Callback, int> callbacksToRemove = new Dictionary<Callback, int>();
            foreach (Callback callback in this.activeCallbacks.Keys.ToList())
            {
                this.activeCallbacks[callback] -= ms;

                if (this.activeCallbacks[callback] <= 0)
                {
                    callbacksToRemove[callback] = callback();
                }
            }

            foreach (KeyValuePair<Callback, int> result in callbacksToRemove)
                if (result.Value < 0)
                    this.activeCallbacks.Remove(result.Key);
                else
                    this.activeCallbacks[result.Key] = result.Value;
        }

        public delegate int Callback();
    }
}