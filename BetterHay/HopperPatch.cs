using Harmony;
using StardewValley;
using System;
using System.Reflection;
using SObject = StardewValley.Object;

namespace BetterHay
{
    [HarmonyPatch]
    internal class HopperPatch
    {
        private static MethodBase TargetMethod()
        {
            return typeof(StardewValley.Object).GetMethod("checkForAction");
        }

        private static bool Prefix(SObject __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
        {
            if (!__instance.name.Contains("Hopper") || who.ActiveObject != null)
                return true;

            __result = CheckForAction(__instance, who, justCheckingForActivity);
            return false;
        }

        //Same as the original method, but pull out 1 hay instead of 0.
        private static bool CheckForAction(SObject instance, Farmer who, bool justCheckingForActivity)
        {
            if (justCheckingForActivity || who == null)
                return true;

            if (who.freeSpotsInInventory() > 0)
            {
                int piecesOfHay = Game1.getFarm().piecesOfHay.Value;
                if (piecesOfHay > 0)
                {
                    if (who.currentLocation is AnimalHouse animalHouse)
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
                                instance.showNextIndex.Value = false;
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
                        instance.showNextIndex.Value = false;
                }
                else
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12942"));
            }
            else
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));

            return false;
        }
    }
}
