using StardewModdingAPI;
using System.Collections.Generic;

namespace BetterDoors.Framework.Utility
{
    /// <summary>Queues error message for nicer display.</summary>
    internal class ErrorQueue
    {
        /*********
        ** Fields
        *********/

        /// <summary>The current list of errors.</summary>
        private readonly IList<string> errors = new List<string>();

        /// <summary>Encapsulates monitoring and logging for a given module.</summary>
        private readonly IMonitor monitor;

        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging for a given module.</param>
        public ErrorQueue(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        /// <summary>Adds an error to be displayed later.</summary>
        /// <param name="error">The error to add.</param>
        public void AddError(string error)
        {
            this.errors.Add($"\t- {error}");
        }

        /// <summary>Prints out all previously added errors.</summary>
        /// <param name="title">A title for this set of errors.</param>
        public void PrintErrors(string title)
        {
            if (this.errors.Count == 0)
                return;

            this.errors.Insert(0, title);
            Utils.LogContentPackError(this.monitor, string.Join("\n", this.errors));
            this.errors.Clear();
        }
    }
}
