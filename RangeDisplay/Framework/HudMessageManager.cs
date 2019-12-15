using RangeDisplay.Framework.RangeHandling;
using StardewValley;
using System;
using System.Text.RegularExpressions;

namespace RangeDisplay.Framework
{
    /// <summary>Manages displaying messages in the game hud.</summary>
    internal class HudMessageManager
    {
        /*********
        ** Fields
        *********/

        /// <summary>The last sent message</summary>
        private HUDMessage lastMessage;

        private static readonly Regex NameRegex = new Regex(@"(?<!^)(?=[A-Z])", RegexOptions.Compiled);

        /*********
        ** Public methods
        *********/

        /// <summary>Adds the given message to the hud.</summary>
        /// <param name="message">The message to show.</param>
        public void AddHudMessage(string message)
        {
            if (this.lastMessage != null && Game1.hudMessages.Contains(this.lastMessage))
            {
                Game1.hudMessages.Remove(this.lastMessage);
            }

            this.lastMessage = new HUDMessage("Range Display: " + message, "");
            Game1.addHUDMessage(this.lastMessage);
        }

        /// <summary>Adds a message to the hud based on the item.</summary>
        /// <param name="item">The item to create the message for.</param>
        public void AddHudMessage(RangeItem item)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            string name = string.Join(" ", HudMessageManager.NameRegex.Split(Enum.GetName(typeof(RangeItem), item)));

            this.AddHudMessage(name + "s");
        }
    }
}
