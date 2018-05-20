using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;
namespace BetterGardenPots.Patches.IndoorPot
{
    internal class PerformToolActionPatch
    {
        private static Crop crop;

        public static void Prefix(StardewValley.Objects.IndoorPot __instance)
        {
            crop = __instance.hoeDirt.Value?.crop;
        }

        public static void Postfix(StardewValley.Objects.IndoorPot __instance, GameLocation location)
        {
            if (__instance.hoeDirt.Value?.crop == null && crop != null && crop.currentPhase.Value == crop.phaseDays.Count - 1)         
                location.debris.Add(new Debris(new SObject(crop.indexOfHarvest.Value, 1), __instance.TileLocation * 64f + new Vector2(32f, 32f)));
        }
    }
}
