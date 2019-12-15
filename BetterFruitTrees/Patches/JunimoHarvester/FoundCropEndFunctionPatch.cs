using Microsoft.Xna.Framework;
using StardewValley;

namespace BetterFruitTrees.Patches.JunimoHarvester
{
    /// <summary>Make fruit trees count as places for the junimo to stop.</summary>
    internal class FoundCropEndFunctionPatch
    {
        public static void Postfix(ref bool __result, PathNode currentNode, Point endPoint, GameLocation location,
            Character c)
        {
            if (__result)
                return;
            __result = Utils.IsAdjacentReadyToHarvestFruitTree(new Vector2(currentNode.x, currentNode.y), location);
        }
    }
}
