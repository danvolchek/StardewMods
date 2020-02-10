using Newtonsoft.Json;
using StardewValley;
using System.Linq;
using StardewModdingAPI;

namespace BetterArtisanGoodIcons.Framework.Data.Format
{
    [JsonConverter(typeof(ItemIndicatorConverter))]
    internal class ItemIndicator
    {
        private const int Unloaded = -999;

        private int Value { get; set; } = Unloaded;

        public string Name { get; }

        public ItemIndicator(string value)
        {
            this.Name = value;
        }

        public ItemIndicator(int value)
        {
            this.Value = value;
        }

        public bool TryLoad(out int value)
        {
            if (this.Name != null)
            {
                this.Value = ItemIndicator.GetIdFromName(this.Name);
            }

            value = this.Value;

            return this.Value != Unloaded;
        }

        public ItemIndicator Clone()
        {
            return this.Name != null ? new ItemIndicator(this.Name) : new ItemIndicator(this.Value);
        }

        public bool AreEqual(ItemIndicator other)
        {
            return this.Name != null ? this.Name == other.Name : this.Value == other.Value;
        }

        public override string ToString()
        {
            return this.Name != null ? $"{this.Name} ({this.Value})" : $"{this.Value}";
        }

        public void LoadErrorMessage(IMonitor monitor, string type)
        {
            if(Context.IsWorldReady)
                monitor.Log($"Failed to get the real item id for {type} {this}. If this is a custom item, make sure the mod that adds it is installed. If it's not, try resetting your XNB files to fix data corruption.", LogLevel.Debug);
        }

        private static int GetIdFromName(string name)
        {
            int[] matches = Game1.objectInformation?.Where(kvp => kvp.Value.Split('/')[0] == name).Select(kvp => kvp.Key).ToArray();

            return matches?.Length == 1 ? matches[0] : Unloaded;
        }
    }
}
