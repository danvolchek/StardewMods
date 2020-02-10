using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace BetterArtisanGoodIcons.Framework.Patches.SDVObject
{
    [HarmonyPatch]
    internal class DrawInMenuPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(SObject).GetMethod(nameof(SObject.drawInMenu), new []{typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool)});
        }

        /// <summary>Draw the correct texture based on <see cref="SObject.preservedParentSheetIndex"/> or <see cref="SObject.Name"/>.</summary>
        private static bool Prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, Color color, StackDrawType drawStackNumber, bool drawShadow)
        {
            if (!ModEntry.Instance.GetDrawInfo(__instance, out Texture2D spriteSheet, out Rectangle position, out Rectangle iconPosition))
                return true;

            if (__instance.IsRecipe)
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }
            bool flag = (drawStackNumber == StackDrawType.Draw && __instance.maximumStackSize() > 1 && __instance.Stack > 1 || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && __instance.Stack != int.MaxValue;
            if (__instance.IsRecipe)
                flag = false;
            if (__instance.bigCraftable.Value)
            {
                Microsoft.Xna.Framework.Rectangle rectForBigCraftable = SObject.getSourceRectForBigCraftable(__instance.ParentSheetIndex);
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, location + new Vector2(32f, 32f), new Microsoft.Xna.Framework.Rectangle?(rectForBigCraftable), color * transparency, 0.0f, new Vector2(8f, 16f), (float)(4.0 * ((double)scaleSize < 0.2 ? (double)scaleSize : (double)scaleSize / 2.0)), SpriteEffects.None, layerDepth);
                if (flag)
                    Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)) + 3f * scaleSize, (float)(64.0 - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, color);
            }
            else
            {
                if (__instance.ParentSheetIndex != 590 & drawShadow)
                    spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), color * 0.5f, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
                // modified
                spriteBatch.Draw(spriteSheet, location + new Vector2((float)(int)(32.0 * (double)scaleSize), (float)(int)(32.0 * (double)scaleSize)), new Microsoft.Xna.Framework.Rectangle?(position), color * transparency, 0.0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
                if (flag)
                    Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)) + 3f * scaleSize, (float)(64.0 - 18.0 * (double)scaleSize + 1.0)), 3f * scaleSize, 1f, color);
                
                // New
                if (transparency >= 1 && iconPosition != Rectangle.Empty)
                    spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(Game1.tileSize / 6 * scaleSize, Game1.tileSize / 6 * scaleSize), new Microsoft.Xna.Framework.Rectangle?(iconPosition), Color.White * transparency, 0.0f, new Vector2(4f, 4f), (3f / 2f) * scaleSize, SpriteEffects.None, layerDepth);
                
                if (drawStackNumber != StackDrawType.Hide && __instance.Quality > 0)
                {
                    float num = __instance.Quality < 4 ? 0.0f : (float)((Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
                    spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, 52f + num), new Microsoft.Xna.Framework.Rectangle?(__instance.Quality < 4 ? new Microsoft.Xna.Framework.Rectangle(338 + (__instance.Quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8)), color * transparency, 0.0f, new Vector2(4f, 4f), (float)(3.0 * (double)scaleSize * (1.0 + (double)num)), SpriteEffects.None, layerDepth);
                }
                if (__instance.Category == -22 && __instance.uses.Value > 0)
                {
                    float power = ((float)(FishingRod.maxTackleUses - __instance.uses.Value) + 0.0f) / (float)FishingRod.maxTackleUses;
                    spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)location.X, (int)((double)location.Y + 56.0 * (double)scaleSize), (int)(64.0 * (double)scaleSize * (double)power), (int)(8.0 * (double)scaleSize)), Utility.getRedToGreenLerpColor(power));
                }
            }
            if (__instance.IsRecipe)
                spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(16f, 16f), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16)), color, 0.0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth + 0.0001f);

            return false;
        }
    }
}
