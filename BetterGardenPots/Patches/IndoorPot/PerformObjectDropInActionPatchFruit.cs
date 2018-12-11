using StardewValley;
using System.Linq;

namespace BetterGardenPots.Patches.IndoorPot
{
    class PerformObjectDropInActionPatchFruit
    {
        public static void Postfix(StardewValley.Objects.IndoorPot __instance, ref bool __result, Item dropInItem,
            bool probe, Farmer who)
        {
            if (__result || dropInItem == null || dropInItem.ParentSheetIndex != 499)
                return;
            if (who == null || !__instance.hoeDirt.Value.canPlantThisSeedHere(
                    dropInItem.ParentSheetIndex, (int) __instance.TileLocation.X, (int) __instance.TileLocation.Y,
                    dropInItem.Category == -19))
                return;

            if (!probe)
            {
                __instance.hoeDirt.Value.plant(dropInItem.ParentSheetIndex, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y, who,
                    dropInItem.Category == -19, who.currentLocation);
                Game1.hudMessages.Remove(Game1.hudMessages.FirstOrDefault(item =>
                    item.Message == Game1.parseText(Game1.content.LoadString("Strings\\Objects:AncientFruitPot"),
                        Game1.dialogueFont, 384)));
            }
            else
                __instance.heldObject.Value = new StardewValley.Object();

            __result = true;
        }
    }
}
