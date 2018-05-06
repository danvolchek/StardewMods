using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Linq;
using SObject = StardewValley.Object;

namespace StackEverything.Patches
{
    /// <summary>Draw stack size in inventories.</summary>
    internal class DrawInMenuPatch
    {
        public static void Postfix(Item __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            if (!(__instance is SObject objInstance && !objInstance.bigCraftable) && !StackEverythingMod.PatchedTypes.Any(item => __instance.GetType().IsAssignableFrom(item)))
                return;

            if (drawStackNumber && __instance.maximumStackSize() > 1 && ((double)scaleSize > 0.3 && __instance.Stack != int.MaxValue) && __instance.Stack > 1)
                Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
        }
    }
}