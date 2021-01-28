using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace HoldToBreakGeodes
{
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/

        /// <summary>The last clicked x position.</summary>
        private int leftClickXPos;

        /// <summary>The last clicked y position.</summary>
        private int leftClickYPos;

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        /*********
        ** Private methods
        *********/

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
            if (e.IsMultipleOf(4) && this.Helper.Input.IsDown(SButton.MouseLeft) && Game1.activeClickableMenu is GeodeMenu menu && !menu.waitingForServerResponse)
            {
                bool clintNotBusy = menu.heldItem != null && Utility.IsGeode(menu.heldItem, false) && Game1.player.Money >= 25 && menu.geodeAnimationTimer <= 0;

                bool playerHasRoom = Game1.player.freeSpotsInInventory() > 1 || (Game1.player.freeSpotsInInventory() == 1 && menu.heldItem != null && menu.heldItem.Stack == 1);

                if (clintNotBusy && playerHasRoom && menu.geodeSpot.containsPoint(this.leftClickXPos, this.leftClickYPos))
                {
                    menu.receiveLeftClick(this.leftClickXPos, this.leftClickYPos, false);
                }
            }
        }
    }
}