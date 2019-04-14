using StardewModdingAPI;
using System.Collections.Generic;

namespace BetterDoors.Framework.Utility
{
    internal class ContentPackErrorManager
    {
        private readonly IList<string> errors = new List<string>();
        private readonly IMonitor monitor;

        public ContentPackErrorManager(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public void AddError(string error)
        {
            this.errors.Add($"\t- {error}");
        }

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
