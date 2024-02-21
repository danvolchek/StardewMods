using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using SObject = StardewValley.Object;

namespace BetterArtisanGoodIcons
{
    internal static class Patches
    {
        private static IMonitor Monitor;

        // Call this method from Entry class so we can monitor patches for errors
        internal static void Init(IMonitor monitor)
        {
            Monitor = monitor;
        }

        internal static class SObjectPatches
        {
            /// <summary>Draw the correct texture based on <see cref="SObject.preservedParentSheetIndex"/> or <see cref="SObject.Name"/>.</summary>
            public static bool DrawWhenHeld_Prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
            {
                try
                {
                    if (!ArtisanGoodsManager.GetDrawInfo(__instance, out Texture2D spriteSheet, out Rectangle position, out Rectangle iconPosition))
                    {
                        return true;
                    }

                    spriteBatch.Draw(spriteSheet, objectPosition, position, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + 3) / 10000f));
                    
                    return false;
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed in {nameof(SObjectPatches)}.{nameof(DrawWhenHeld_Prefix)}:\n{ex}", LogLevel.Error);

                    return true;
                }
            }

            /// <summary>Draw the correct texture based on <see cref="SObject.preservedParentSheetIndex"/> or <see cref="SObject.Name"/>.</summary>
            public static bool DrawInMenu_Prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                try
                {
                    if (!ArtisanGoodsManager.GetDrawInfo(__instance, out Texture2D spriteSheet, out Rectangle position, out Rectangle iconPosition))
                    {
                        return true;
                    }

                    spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(Game1.tileSize / 2, Game1.tileSize * 3 / 4), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * 0.5f, 0.0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);

                    spriteBatch.Draw(spriteSheet, location + new Vector2(Game1.tileSize / 2 * scaleSize, Game1.tileSize / 2 * scaleSize), new Microsoft.Xna.Framework.Rectangle?(position), Color.White * transparency, 0.0f, new Vector2(8f, 8f) * scaleSize, Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);

                    //By popular demand, don't show icons near the mouse cursor, which are drawn with lowered transparency.
                    if (transparency >= 1 && iconPosition != Rectangle.Empty)
                    {
                        spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(Game1.tileSize / 6 * scaleSize, Game1.tileSize / 6 * scaleSize), new Microsoft.Xna.Framework.Rectangle?(iconPosition), Color.White * transparency, 0.0f, new Vector2(4f, 4f), (3f / 2f) * scaleSize, SpriteEffects.None, layerDepth);
                    }

					bool flag = ((drawStackNumber == StackDrawType.Draw && __instance.maximumStackSize() > 1 && __instance.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && __instance.Stack != int.MaxValue;

					if (flag)
                    {
                        Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2(Game1.tileSize - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize) + 3f * scaleSize, Game1.tileSize - 18.0f * scaleSize + 1f), 3f * scaleSize, 1f, Color.White);
                    }

                    if (drawStackNumber != 0 && __instance.Quality > 0)
                    {
						Rectangle value = (__instance.Quality < 4 ? new Rectangle(338 + (__instance.Quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8));
                        float num = __instance.Quality < 4 ? 0.0f : (float)((Math.Cos(Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.05f);
                        spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, Game1.tileSize - 12 + num), value, Color.White * transparency, 0.0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + num), SpriteEffects.None, layerDepth);
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed in {nameof(SObjectPatches)}.{nameof(DrawInMenu_Prefix)}:\n{ex}", LogLevel.Error);

                    return true;
                }
            }

            /// <summary>Draw the correct texture for machines that are ready for harvest and displaying their results.</summary>
            /// <remarks>We can't just set <see cref="SObject.readyForHarvest"/> to be false during the draw call and draw the 
            /// tool tip ourselves because draw calls getScale, which actually modifies the object scale based upon <see cref="SObject.readyForHarvest"/>.</remarks>
            public static bool Draw_Prefix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
            {
                try
                {
                    if (!__instance.readyForHarvest.Value || !__instance.bigCraftable.Value || __instance.heldObject.Value == null
                        || !ArtisanGoodsManager.GetDrawInfo(__instance.heldObject.Value, out Texture2D spriteSheet, out Rectangle position, out Rectangle iconPosition))
                    {
                        return true;
                    }

                    Vector2 vector = __instance.getScale() * Game1.pixelZoom;
                    Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize, y * Game1.tileSize - Game1.tileSize));
                    Rectangle destinationRectangle = new Rectangle((int)(local.X - vector.X / 2f) + (__instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(local.Y - vector.Y / 2f) + (__instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(Game1.tileSize + vector.X), (int)((Game1.tileSize * 2f) + vector.Y / 2f));

					float num = Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f;
					
                    if (__instance.ParentSheetIndex == 105 || __instance.ParentSheetIndex == 264)
					{
						num = Math.Max(0f, ((y + 1) * 64 + 2) / 10000f) + x / 1000000f;
					}

					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, new Microsoft.Xna.Framework.Rectangle?(SObject.getSourceRectForBigCraftable(__instance.showNextIndex.Value ? __instance.ParentSheetIndex + 1 : __instance.ParentSheetIndex)), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, num);

					if (__instance.Name.Equals("Loom") && __instance.MinutesUntilReady > 0)
                    {
                        spriteBatch.Draw(Game1.objectSpriteSheet, __instance.getLocalPosition(Game1.viewport) + new Vector2(Game1.tileSize / 2f, 0f), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16)), Color.White * alpha, __instance.scale.X, new Vector2(8f, 8f), Game1.pixelZoom, SpriteEffects.None, Math.Max(0f, ((y + 1) * Game1.tileSize) / 10000f + 0.0001f + x * 1E-05f));
                    }

                    if (__instance.isLamp.Value && Game1.isDarkOut())
                    {
                        spriteBatch.Draw(Game1.mouseCursors, local + new Vector2(-Game1.tileSize / 2f, -Game1.tileSize / 2f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32)), Color.White * 0.75f, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0f, ((y + 1) * Game1.tileSize - Game1.pixelZoom * 5) / 10000f) + x / 1000000f);
					}

                    if (__instance.ParentSheetIndex == 126 && __instance.Quality != 0)
                    {
                        spriteBatch.Draw(FarmerRenderer.hatsTexture, local + new Vector2(-3f, -6f) * Game1.pixelZoom, new Rectangle((__instance.Quality - 1) * 20 % FarmerRenderer.hatsTexture.Width, (__instance.Quality - 1) * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), Color.White * alpha, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0f, ((y + 1) * Game1.tileSize - Game1.pixelZoom * 5) / 10000f) + x * 1E-05f);
                    }

					float num2 = (y + 1) * Game1.tileSize / 10000f + __instance.TileLocation.X / 50000f;

					if (__instance.ParentSheetIndex == 105 || __instance.ParentSheetIndex == 264)
					{
						num2 += 0.02f;
					}

					float num3 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize - 8, y * Game1.tileSize - Game1.tileSize * 3 / 2 - 16 + num3)), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, num2 + 1E-06f);
					spriteBatch.Draw(spriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize + Game1.tileSize / 2, y * Game1.tileSize - Game1.tileSize - Game1.tileSize / 8 + num3)), new Microsoft.Xna.Framework.Rectangle?(position), Color.White * 0.75f, 0f, new Vector2(8f, 8f), Game1.pixelZoom, SpriteEffects.None, num2 + 1E-05f);

					return false;
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed in {nameof(SObjectPatches)}.{nameof(Draw_Prefix)}:\n{ex}", LogLevel.Error);

                    return true;
                }
            }
        }
        internal static class FurniturePatches
        {
            /// <summary>Draw the right texture for the object placed on furniture.</summary>
            public static bool Draw_Prefix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
            {
                try
                {
					if (__instance.heldObject.Value == null || __instance.heldObject.Value is Furniture
						|| !ArtisanGoodsManager.GetDrawInfo(__instance.heldObject.Value, out Texture2D spriteSheet, out Rectangle position, out Rectangle iconPosition))
					{
						return true;
					}

                    if (x == -1)
                    {
                        Vector2 drawPos = new Vector2(__instance.boundingBox.X, __instance.boundingBox.Y - (__instance.sourceRect.Height * Game1.pixelZoom - __instance.boundingBox.Value.Height));
                        spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, drawPos), new Rectangle?(__instance.sourceRect.Value), Color.White * alpha, 0.0f, Vector2.Zero, Game1.pixelZoom, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, __instance.furniture_type.Value == 12 ? (2E-09f + __instance.TileLocation.Y / 100000f) : (__instance.boundingBox.Value.Bottom - 8) / 10000f);
                    }
                    else
                    {
                        spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize, y * Game1.tileSize - (__instance.sourceRect.Value.Height * Game1.pixelZoom - __instance.boundingBox.Value.Height))), new Rectangle?(__instance.sourceRect.Value), Color.White * alpha, 0f, Vector2.Zero, Game1.pixelZoom, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, __instance.furniture_type.Value == 12 ? (2E-09f + __instance.TileLocation.Y / 100000f) : (__instance.boundingBox.Value.Bottom - 8) / 10000f);
                    }

                    spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.boundingBox.Value.Center.X - Game1.tileSize / 2, __instance.boundingBox.Value.Center.Y - (__instance.drawHeldObjectLow.Value ? Game1.tileSize / 2 : Game1.tileSize * 4 / 3))) + new Vector2(Game1.tileSize / 2f, Game1.tileSize * 5f / 6), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), Game1.pixelZoom, SpriteEffects.None, __instance.boundingBox.Value.Bottom / 10000f);
                    spriteBatch.Draw(spriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.boundingBox.Value.Center.X - Game1.tileSize / 2, __instance.boundingBox.Value.Center.Y - (__instance.drawHeldObjectLow.Value ? Game1.tileSize / 2 : Game1.tileSize * 4 / 3))), new Rectangle?(position), Color.White * alpha, 0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, (__instance.boundingBox.Value.Bottom + 1) / 10000f);

                    return false;
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Failed in {nameof(FurniturePatches)}.{nameof(Draw_Prefix)}:\n{ex}", LogLevel.Error);

                    return true;
                }
            }
        }
    }
}
