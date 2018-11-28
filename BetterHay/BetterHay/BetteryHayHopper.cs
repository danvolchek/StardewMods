using Microsoft.Xna.Framework;
using StardewValley;
using System;
using SObject = StardewValley.Object;
namespace BetterHay
{
    public class BetterHayHopper : SObject
    {
        public BetterHayHopper() : base(Vector2.Zero, 99, false)
        {

        }
        public BetterHayHopper(Vector2 tileLocation, int parentSheetIndex, bool isRecipe) : base(tileLocation, parentSheetIndex, false)
        {
            if (Game1.getFarm().piecesOfHay.Value > 0)
                this.showNextIndex.Value = true;
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity || who == null)
                return true;

            if ((who.currentLocation.isObjectAt(who.getTileX(), who.getTileY() - 1) && who.currentLocation.isObjectAt(who.getTileX(), who.getTileY() + 1)) && (who.currentLocation.isObjectAt(who.getTileX() + 1, who.getTileY()) && who.currentLocation.isObjectAt(who.getTileX() - 1, who.getTileY())))
                this.performToolAction((Tool)null, who.currentLocation);

            
            if (this.name.Contains("Hopper") && who.ActiveObject == null)
            {
             
                if (who.freeSpotsInInventory() > 0)
                {
                    int piecesOfHay = Game1.getFarm().piecesOfHay.Value;
                    if (piecesOfHay > 0)
                    {
                        if (Game1.currentLocation is AnimalHouse animalHouse)
                        {
                            int val1 = Math.Max(1, Math.Min(animalHouse.animalsThatLiveHere.Count, piecesOfHay));
                            int num1 = animalHouse.numberOfObjectsWithName("Hay");
                            int num2 = Math.Min(val1, animalHouse.animalLimit.Value - num1);
                            //##CHANGES
                            if (num2 == 0)
                                num2 = 1;
                            if (Game1.player.couldInventoryAcceptThisObject(178, num2, 0))
                            {
                                Game1.getFarm().piecesOfHay.Value -= Math.Max(1, num2);
                                who.addItemToInventoryBool((Item)new SObject(178, num2, false, -1, 0), false);
                                Game1.playSound("shwip");
                                if (Game1.getFarm().piecesOfHay.Value <= 0)
                                    this.showNextIndex.Value = false;
                                return true;
                            }
                        }
                        else if (Game1.player.couldInventoryAcceptThisObject(178, 1, 0))
                        {
                            --Game1.getFarm().piecesOfHay.Value;
                            who.addItemToInventoryBool((Item)new SObject(178, 1, false, -1, 0), false);
                            Game1.playSound("shwip");
                        }
                        if (Game1.getFarm().piecesOfHay.Value <= 0)
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
