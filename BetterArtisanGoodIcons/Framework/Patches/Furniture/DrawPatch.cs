using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;

namespace BetterArtisanGoodIcons.Framework.Patches.Furniture
{
    /// <summary>Draw the right texture for the object placed on furniture.</summary>
    [HarmonyPatch]
    internal class DrawPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(StardewValley.Objects.Furniture).GetMethod(nameof(StardewValley.Objects.Furniture.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) });
        }

        public static bool Prefix(StardewValley.Objects.Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (!ModEntry.Instance.GetDrawInfo(__instance.heldObject.Value, out Texture2D spriteSheet, out Rectangle bagiPosition, out Rectangle iconPosition))
                return true;

            if (!__instance.isTemporarilyInvisible)
            {
                if (StardewValley.Objects.Furniture.isDrawingLocationFurniture)
                {
                    Vector2 drawPosition = Traverse.Create(__instance).Field<NetVector2>("drawPosition").Value.Value;
                    spriteBatch.Draw(StardewValley.Objects.Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, (Vector2)drawPosition + (__instance.shakeTimer > 0 ? new Vector2((float) Game1.random.Next(-1, 2), (float) Game1.random.Next(-1, 2)) : Vector2.Zero)), new Rectangle?((Rectangle) (__instance.sourceRect)), Color.White * alpha, 0.0f, Vector2.Zero, 4f, (bool) (__instance.flipped) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (int) (__instance.furniture_type) == 12 ? 2E-09f : (float) (__instance.boundingBox.Value.Bottom - ((int) (__instance.furniture_type) == 6 || (int) (__instance.furniture_type) == 13 ? 48 : 8)) / 10000f);
                }else
                    spriteBatch.Draw(StardewValley.Objects.Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float) (x * 64 + (__instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)), (float) (y * 64 - (__instance.sourceRect.Height * 4 - __instance.boundingBox.Height) + (__instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)))), new Rectangle?((Rectangle) (__instance.sourceRect)), Color.White * alpha, 0.0f, Vector2.Zero, 4f, (bool) (__instance.flipped) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (int) (__instance.furniture_type) == 12 ? 2E-09f : (float) (__instance.boundingBox.Value.Bottom - ((int) (__instance.furniture_type) == 6 || (int) (__instance.furniture_type) == 13 ? 48 : 8)) / 10000f);
                if (__instance.heldObject.Value != null)
                {
                    if (__instance.heldObject.Value is StardewValley.Objects.Furniture)
                    {
                        (__instance.heldObject.Value as StardewValley.Objects.Furniture).drawAtNonTileSpot(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float) (__instance.boundingBox.Center.X - 32), (float) (__instance.boundingBox.Center.Y - (__instance.heldObject.Value as StardewValley.Objects.Furniture).sourceRect.Height * 4 - ((bool) (__instance.drawHeldObjectLow) ? -16 : 16)))), (float) (__instance.boundingBox.Bottom - 7) / 10000f, alpha);
                    }
                    else
                    {
                        SpriteBatch spriteBatch1 = spriteBatch;
                        Texture2D shadowTexture = Game1.shadowTexture;
                        Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float) (__instance.boundingBox.Center.X - 32), (float) (__instance.boundingBox.Center.Y - ((bool) (__instance.drawHeldObjectLow) ? 32 : 85)))) + new Vector2(32f, 53f);
                        Rectangle? sourceRectangle = new Rectangle?(Game1.shadowTexture.Bounds);
                        Color color = Color.White * alpha;
                        Rectangle bounds = Game1.shadowTexture.Bounds;
                        double x1 = (double) bounds.Center.X;
                        bounds = Game1.shadowTexture.Bounds;
                        double y1 = (double) bounds.Center.Y;
                        Vector2 origin = new Vector2((float) x1, (float) y1);
                        double num = (double)__instance.boundingBox.Bottom / 10000.0;
                        spriteBatch1.Draw(shadowTexture, position, sourceRectangle, color, 0.0f, origin, 4f, SpriteEffects.None, (float) num);
                        spriteBatch.Draw(spriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float) (__instance.boundingBox.Center.X - 32), (float) (__instance.boundingBox.Center.Y - ((bool) (__instance.drawHeldObjectLow) ? 32 : 85)))), bagiPosition, Color.White * alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float) (__instance.boundingBox.Bottom + 1) / 10000f);
                    }
                }

                if ((bool) (__instance.IsOn) && (int) (__instance.furniture_type.Value) == 14)
                {
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float) (__instance.boundingBox.Center.X - 12), (float) (__instance.boundingBox.Center.Y - 64))), new Rectangle?(new Rectangle(276 + (int) ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double) (x * 3047) + (double) (y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float) (__instance.getBoundingBox(new Vector2((float) x, (float) y)).Bottom - 2) / 10000f);
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float) (__instance.boundingBox.Center.X - 32 - 4), (float) (__instance.boundingBox.Center.Y - 64))), new Rectangle?(new Rectangle(276 + (int) ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double) (x * 2047) + (double) (y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float) (__instance.getBoundingBox(new Vector2((float) x, (float) y)).Bottom - 1) / 10000f);
                }

                if (Game1.debugMode)
                {
                    Vector2 drawPosition = Traverse.Create(__instance).Field<NetVector2>("drawPosition").Value.Value;
                    spriteBatch.DrawString(Game1.smallFont, string.Concat((object) __instance.ParentSheetIndex), Game1.GlobalToLocal(Game1.viewport, (Vector2)drawPosition), Color.Yellow, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                }
            }
            return false;
        }
    }
}
