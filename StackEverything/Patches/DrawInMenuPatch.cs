using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Linq;

namespace StackEverything.Patches
{
    /// <summary>Draw stack size in inventories.</summary>
    internal class DrawInMenuPatch
    {
        public static void Postfix(Item __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, StackDrawType drawStackNumber)
        {
            if (!StackEverythingMod.PatchedTypes.Any(item => item.IsInstanceOfType(__instance)))
                return;

            if (((drawStackNumber == StackDrawType.Draw && __instance.maximumStackSize() > 1 && __instance.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && __instance.Stack != int.MaxValue)
                Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
        }
    }
}
