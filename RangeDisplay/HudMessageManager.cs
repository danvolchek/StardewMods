using RangeDisplay.RangeHandling;
using StardewValley;
using System;

namespace RangeDisplay
{
    internal class HudMessageManager
    {
        private HUDMessage currentMessage = null;

        public void AddHudMessage(string message)
        {
            if (this.currentMessage != null && Game1.hudMessages.Contains(this.currentMessage))
            {
                Game1.hudMessages.Remove(this.currentMessage);
            }

            this.currentMessage = new HUDMessage("Range Display: " + message, "");
            Game1.addHUDMessage(this.currentMessage);
        }

        public void AddHudMessage(RangeItem item)
        {
            string name = Enum.GetName(typeof(RangeItem), item);
            name = name.Replace('_', ' ');

            AddHudMessage(name + "s");
        }
    }
}