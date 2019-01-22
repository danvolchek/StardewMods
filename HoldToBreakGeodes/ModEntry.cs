using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace HoldToBreakGeodes
{
    public class ModEntry : Mod
    {
        private int leftClickXPos;
        private int leftClickYPos;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // record the left-click position
            if (e.Button == SButton.MouseLeft)
            {
                this.leftClickXPos = (int)e.Cursor.ScreenPixels.X;
                this.leftClickYPos = (int)e.Cursor.ScreenPixels.Y;
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Re-send a left click to the geode menu if one is already not being broken, the player has the room and money for it, and the click was on the geode spot.
            if (e.IsMultipleOf(4) && this.Helper.Input.IsDown(SButton.MouseLeft) && Game1.activeClickableMenu is GeodeMenu menu)
            {
                bool clintNotBusy =
                    menu.heldItem != null
                    && menu.heldItem.Name.Contains("Geode")
                    && Game1.player.money >= 25 && menu.geodeAnimationTimer <= 0;

                bool playerHasRoom =
                    Game1.player.freeSpotsInInventory() > 1
                    || (Game1.player.freeSpotsInInventory() == 1 && menu.heldItem != null && menu.heldItem.Stack == 1);

                if (clintNotBusy && playerHasRoom && menu.geodeSpot.containsPoint(this.leftClickXPos, this.leftClickYPos))
                    menu.receiveLeftClick(this.leftClickXPos, this.leftClickYPos, false);
            }
        }
    }
}