using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using SObject = StardewValley.Object;

namespace BetterArtisanGoodIcons.Framework.Patches.SDVObject
{
    [HarmonyPatch]
    internal class DrawWhenHeldPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(SObject).GetMethod(nameof(SObject.drawWhenHeld));
        }

        /// <summary>Draw the correct texture based on <see cref="SObject.preservedParentSheetIndex"/> or <see cref="SObject.Name"/>.</summary>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (!ModEntry.Instance.GetDrawInfo(__instance, out Texture2D spriteSheet, out Rectangle position, out Rectangle iconPosition))
                return true;

            //By popular demand, don't show icons when held.

            if (f.ActiveObject.bigCraftable.Value)
            {
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, objectPosition, new Microsoft.Xna.Framework.Rectangle?(SObject.getSourceRectForBigCraftable(f.ActiveObject.ParentSheetIndex)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 3) / 10000f));
            }
            else
            {
                spriteBatch.Draw(spriteSheet, objectPosition, position, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 3) / 10000f));
                if (!(f.ActiveObject == null || !f.ActiveObject.Name.Contains("=")))
                {
                    spriteBatch.Draw(spriteSheet, objectPosition + new Vector2(32f, 32f), position, Color.White, 0.0f, new Vector2(32f, 32f), (float) (4.0 + (double) Math.Abs(Game1.starCropShimmerPause) / 8.0), SpriteEffects.None, Math.Max(0.0f, (float) (f.getStandingY() + 3) / 10000f));
                    if (!((double) Math.Abs(Game1.starCropShimmerPause) <= 0.0500000007450581 && Game1.random.NextDouble() < 0.97))
                    {
                        Game1.starCropShimmerPause += 0.04f;
                        if (!(Game1.starCropShimmerPause < 0.800000011920929))
                        {
                            Game1.starCropShimmerPause = -0.8f;
                        }
                    }
                }
            }

            return false;
        }
    }
}
