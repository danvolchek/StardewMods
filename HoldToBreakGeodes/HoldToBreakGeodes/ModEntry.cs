using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoldToBreakGeodes
{
    public class ModEntry : Mod
    {
        bool leftClickPressed;
        int leftClickXPos;
        int leftClickYPos;

        public override void Entry(IModHelper helper)
        {
            leftClickPressed = false;
            ControlEvents.MouseChanged += this.MouseChanged;
            GameEvents.FourthUpdateTick += this.ReSendLeftClick;
        }

        /**
         * Records the click position of left clicks.
         **/
        public void MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            leftClickPressed = e.NewState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            bool leftClickWasPressed = e.PriorState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
            if (leftClickPressed && !leftClickWasPressed)
            {
                leftClickXPos = e.NewPosition.X;
                leftClickYPos = e.NewPosition.Y;
            }
        }

        /**
         * Re sends a left click to the geode menu if one is already not being broken, the player has the room and money for it, and the click was on the geode spot.
         **/
        public void ReSendLeftClick(object sender, EventArgs e)
        {
            if (leftClickPressed && Game1.activeClickableMenu is StardewValley.Menus.GeodeMenu GMenu)
            {
                bool clintNotBusy = GMenu.heldItem != null && GMenu.heldItem.Name.Contains("Geode") && (Game1.player.money >= 25 && GMenu.geodeAnimationTimer <= 0);
                bool playerHasRoom = Game1.player.freeSpotsInInventory() > 1 || Game1.player.freeSpotsInInventory() == 1 && GMenu.heldItem.Stack == 1;
                if (clintNotBusy && playerHasRoom && GMenu.geodeSpot.containsPoint(leftClickXPos, leftClickYPos))
                {
                    Game1.activeClickableMenu.receiveLeftClick(leftClickXPos, leftClickYPos, false);
                }
            }

        }
    }
}
