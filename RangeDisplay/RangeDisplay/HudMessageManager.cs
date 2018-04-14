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
            if (currentMessage != null && Game1.hudMessages.Contains(currentMessage))
            {
                Game1.hudMessages.Remove(currentMessage);
            }

            currentMessage = new HUDMessage("Range Display: " + message, "");
            Game1.addHUDMessage(currentMessage);
        }

        public void AddHudMessage(RangeItem item)
        {
            string name = Enum.GetName(typeof(RangeItem), item);
            name = name.Replace('_', ' ');

            AddHudMessage(name + "s");
        }
    }
}