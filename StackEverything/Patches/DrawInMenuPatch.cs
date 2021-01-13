using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StackEverything.Patches
{
    /// <summary>Draw stack size in inventories.</summary>
    internal class DrawInMenuPatch
    {
        public static void Postfix(Item instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, bool drawStackNumber)
        {
            if (!StackEverythingMod.PatchedTypes.Any(item => item.IsInstanceOfType(instance)))
                return;

            if (drawStackNumber && instance.maximumStackSize() > 1 && scaleSize > 0.3 && instance.Stack != int.MaxValue && instance.Stack > 1)
                Utility.drawTinyDigits(instance.Stack, spriteBatch, location + new Vector2(Game1.tileSize - Utility.getWidthOfTinyDigitString(instance.Stack, 3f * scaleSize) + 3f * scaleSize, (float)(Game1.tileSize - 18.0 * scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
        }
    }
}
