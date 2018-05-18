using Microsoft.Xna.Framework;
using StardewValley;
using System;
using SObject = StardewValley.Object;
namespace BetterHay
{
    public class BetterHayHopper : SObject
    {
        public BetterHayHopper(Vector2 tileLocation, int parentSheetIndex, bool isRecipe) : base(tileLocation, parentSheetIndex, false)
        {
            if ((Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value > 0)
                this.showNextIndex.Value = true;
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
                return true;

            if (who != null && (who.currentLocation.isObjectAt(who.getTileX(), who.getTileY() - 1) && who.currentLocation.isObjectAt(who.getTileX(), who.getTileY() + 1)) && (who.currentLocation.isObjectAt(who.getTileX() + 1, who.getTileY()) && who.currentLocation.isObjectAt(who.getTileX() - 1, who.getTileY())))
                this.performToolAction((Tool)null, who.currentLocation);

            
            if (this.name.Contains("Hopper") && who.ActiveObject == null)
            {
             
                if (who.freeSpotsInInventory() > 0)
                {
                    int piecesOfHay = (Game1.getLocationFromName("Farm") as Farm).piecesOfHay;
                    if (piecesOfHay > 0)
                    {
                        if (Game1.currentLocation is AnimalHouse)
                        {
                            int val1 = Math.Max(1, Math.Min((Game1.currentLocation as AnimalHouse).animalsThatLiveHere.Count, piecesOfHay));
                            AnimalHouse currentLocation = Game1.currentLocation as AnimalHouse;
                            int num1 = currentLocation.numberOfObjectsWithName("Hay");
                            int num2 = Math.Min(val1, currentLocation.animalLimit - num1);
                            //##CHANGES
                            if (num2 == 0)
                                num2 = 1;
                            if (Game1.player.couldInventoryAcceptThisObject(178, num2, 0))
                            {
                                (Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value -= Math.Max(1, num2);
                                who.addItemToInventoryBool((Item)new SObject(178, num2, false, -1, 0), false);
                                Game1.playSound("shwip");
                            }
                        }
                        else if (Game1.player.couldInventoryAcceptThisObject(178, 1, 0))
                        {
                            --(Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value;
                            who.addItemToInventoryBool((Item)new SObject(178, 1, false, -1, 0), false);
                            Game1.playSound("shwip");
                        }
                        if ((Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value <= 0)
                            this.showNextIndex.Value = false;
                    }
                    else
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12942"));
                }
                else
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
            }


            return false;
        }

    }
}
