using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace BetterFruitTrees.Patches.JunimoHut
{
    internal class AreThereMatureCropsWithinRadiusPatch
    {
        public static void Postfix(ref bool __result, StardewValley.Buildings.JunimoHut __instance)
        {
            if (__result)
                return;

            Farm farm = Game1.getFarm();
            for (int index1 = __instance.tileX.Value + 1 - 8; index1 < __instance.tileX.Value + 2 + 8; ++index1)
            for (int index2 = __instance.tileY.Value - 8 + 1; index2 < __instance.tileY.Value + 2 + 8; ++index2)
            {
                Vector2 possiblePos = new Vector2(index1, index2);

                if (farm.terrainFeatures.ContainsKey(possiblePos) &&
                    farm.terrainFeatures[possiblePos] is FruitTree tree && Utils.CanTreeBeHarvested(tree))
                {
                    Point cropLocation = Utils.GetUnnocupiedAdjacentLocation(index1, index2, farm);
                    if (cropLocation == Point.Zero)
                        continue;
                    __instance.lastKnownCropLocation = cropLocation;
                    __result = true;
                    return;
                }
            }
        }
    }
}