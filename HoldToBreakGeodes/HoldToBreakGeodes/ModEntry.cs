using System;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace HoldToBreakGeodes
{
    public class ModEntry : Mod
    {
        private bool leftClickPressed;
        private int leftClickXPos;
        private int leftClickYPos;

        public override void Entry(IModHelper helper)
        {
            this.leftClickPressed = false;
            ControlEvents.MouseChanged += this.MouseChanged;
            GameEvents.FourthUpdateTick += this.ReSendLeftClick;
        }

        /**
         * Records the click position of left clicks.
         **/
        private void MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            this.leftClickPressed = e.NewState.LeftButton == ButtonState.Pressed;
            bool leftClickWasPressed = e.PriorState.LeftButton == ButtonState.Pressed;
            if (!this.leftClickPressed || leftClickWasPressed) return;

            this.leftClickXPos = e.NewPosition.X;
            this.leftClickYPos = e.NewPosition.Y;
        }

        /**
         * Re sends a left click to the geode menu if one is already not being broken, the player has the room and money for it, and the click was on the geode spot.
         **/
        private void ReSendLeftClick(object sender, EventArgs e)
        {
            if (!this.leftClickPressed || !(Game1.activeClickableMenu is GeodeMenu gMenu)) return;

            bool clintNotBusy = gMenu.heldItem != null && gMenu.heldItem.Name.Contains("Geode") &&
                                Game1.player.money >= 25 && gMenu.geodeAnimationTimer <= 0;
            bool playerHasRoom = Game1.player.freeSpotsInInventory() > 1 ||
                                 (Game1.player.freeSpotsInInventory() == 1 && gMenu.heldItem != null && gMenu.heldItem.Stack == 1);
            if (clintNotBusy && playerHasRoom &&
                gMenu.geodeSpot.containsPoint(this.leftClickXPos, this.leftClickYPos))
                gMenu.receiveLeftClick(this.leftClickXPos, this.leftClickYPos, false);
        }
    }
}