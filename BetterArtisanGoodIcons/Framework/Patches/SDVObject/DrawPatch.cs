using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace BetterArtisanGoodIcons.Framework.Patches.SDVObject
{
    [HarmonyPatch]
    internal class DrawPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(SObject).GetMethod(nameof(SObject.draw), new []{typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)});
        }
        /// <summary>Draw the correct texture for machines that are ready for harvest and displaying their results.</summary>
        /// <remarks>We can't just set <see cref="SObject.readyForHarvest"/> to be false during the draw call and draw the
        /// tool tip ourselves because draw calls getScale, which actually modifies the object scale based upon <see cref="SObject.readyForHarvest"/>.</remarks>
        private static bool Prefix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (!ModEntry.Instance.GetDrawInfo(__instance.heldObject.Value, out Texture2D spriteSheet, out Rectangle position, out Rectangle iconPosition))
                return true;
            
            DrawPatch.Draw(__instance, spriteBatch, x ,y, alpha, spriteSheet, position);
            return false;
        }

        private static void Draw(SObject instance, SpriteBatch spriteBatch, int x, int y, float alpha, Texture2D spriteSheet, Rectangle position)
        {
            if (instance.isTemporarilyInvisible)
                return;
            if ((bool)(instance.bigCraftable))
            {
                Vector2 vector2 = instance.getScale() * 4f;
                Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
                Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)((double)local.X - (double)vector2.X / 2.0) + (instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)local.Y - (double)vector2.Y / 2.0) + (instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(64.0 + (double)vector2.X), (int)(128.0 + (double)vector2.Y / 2.0));
                float layerDepth = Math.Max(0.0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                if (instance.ParentSheetIndex == 105)
                    layerDepth = Math.Max(0.0f, (float)((y + 1) * 64 + 2) / 10000f) + (float)x / 1000000f;
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, new Microsoft.Xna.Framework.Rectangle?(SObject.getSourceRectForBigCraftable((bool)(instance.showNextIndex) ? instance.ParentSheetIndex + 1 : instance.ParentSheetIndex)), Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                if (instance.Name.Equals("Loom") && (int)(instance.minutesUntilReady) > 0)
                    spriteBatch.Draw(Game1.objectSpriteSheet, instance.getLocalPosition(Game1.viewport) + new Vector2(32f, 0.0f), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16)), Color.White * alpha, instance.scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, Math.Max(0.0f, (float)((double)((y + 1) * 64) / 10000.0 + 9.99999974737875E-05 + (double)x * 9.99999974737875E-06)));
                if ((bool)(instance.isLamp) && Game1.isDarkOut())
                    spriteBatch.Draw(Game1.mouseCursors, local + new Vector2(-32f, -32f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32)), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0.0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x / 1000000f);
                if ((int)(instance.parentSheetIndex) == 126 && (int)(instance.quality) != 0)
                    spriteBatch.Draw(FarmerRenderer.hatsTexture, local + new Vector2(-3f, -6f) * 4f, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(((int)(instance.quality) - 1) * 20 % FarmerRenderer.hatsTexture.Width, ((int)(instance.quality) - 1) * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20)), Color.White * alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0.0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f);
            }
            else if (!Game1.eventUp || Game1.CurrentEvent != null && !Game1.CurrentEvent.isTileWalkedOn(x, y))
            {
                if ((int)(instance.parentSheetIndex) == 590)
                {
                    SpriteBatch spriteBatch1 = spriteBatch;
                    Texture2D mouseCursors = Game1.mouseCursors;
                    Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + (instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + (instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0))));
                    Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(368 + (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0 <= 400.0 ? (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 16 : 0), 32, 16, 16));
                    Color color = Color.White * alpha;
                    Vector2 origin = new Vector2(8f, 8f);
                    Vector2 scale = instance.scale;
                    double num1 = (double)instance.scale.Y > 1.0 ? (double)instance.getScale().Y : 4.0;
                    int num2 = (bool)(instance.flipped) ? 1 : 0;
                    double num3 = (instance.isPassable() ? (double)instance.getBoundingBox(new Vector2((float)x, (float)y)).Top : (double)instance.getBoundingBox(new Vector2((float)x, (float)y)).Bottom) / 10000.0;
                    spriteBatch1.Draw(mouseCursors, local, sourceRectangle, color, 0.0f, origin, (float)num1, (SpriteEffects)num2, (float)num3);
                    return;
                }
                Microsoft.Xna.Framework.Rectangle rectangle;
                if ((int)(instance.fragility) != 2)
                {
                    SpriteBatch spriteBatch1 = spriteBatch;
                    Texture2D shadowTexture = Game1.shadowTexture;
                    Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 + 51 + 4)));
                    Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds);
                    Color color = Color.White * alpha;
                    rectangle = Game1.shadowTexture.Bounds;
                    double x1 = (double)rectangle.Center.X;
                    rectangle = Game1.shadowTexture.Bounds;
                    double y1 = (double)rectangle.Center.Y;
                    Vector2 origin = new Vector2((float)x1, (float)y1);
                    rectangle = instance.getBoundingBox(new Vector2((float)x, (float)y));
                    double num = (double)rectangle.Bottom / 15000.0;
                    spriteBatch1.Draw(shadowTexture, local, sourceRectangle, color, 0.0f, origin, 4f, SpriteEffects.None, (float)num);
                }
                SpriteBatch spriteBatch2 = spriteBatch;
                Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
                Vector2 local1 = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32 + (instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 + 32 + (instance.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0))));
                Microsoft.Xna.Framework.Rectangle? sourceRectangle1 = new Microsoft.Xna.Framework.Rectangle?(GameLocation.getSourceRectForObject(instance.ParentSheetIndex));
                Color color1 = Color.White * alpha;
                Vector2 origin1 = new Vector2(8f, 8f);
                Vector2 scale1 = instance.scale;
                double num4 = (double)instance.scale.Y > 1.0 ? (double)instance.getScale().Y : 4.0;
                int num5 = (bool)(instance.flipped) ? 1 : 0;
                int num6;
                if (!instance.isPassable())
                {
                    rectangle = instance.getBoundingBox(new Vector2((float)x, (float)y));
                    num6 = rectangle.Bottom;
                }
                else
                {
                    rectangle = instance.getBoundingBox(new Vector2((float)x, (float)y));
                    num6 = rectangle.Top;
                }
                double num7 = (double)num6 / 10000.0;
                spriteBatch2.Draw(spriteSheet, local1, position, color1, 0.0f, origin1, (float)num4, (SpriteEffects)num5, (float)num7);
            }
            if (!(bool)(instance.readyForHarvest))
                return;
            float num8 = (float)(4.0 * Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2));
            spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 - 8), (float)(y * 64 - 96 - 16) + num8)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((double)((y + 1) * 64) / 10000.0 + 9.99999997475243E-07 + (double)instance.tileLocation.X / 10000.0 + ((int)(instance.parentSheetIndex) == 105 ? 0.00150000001303852 : 0.0)));
            if (instance.heldObject.Value == null)
                return;
            spriteBatch.Draw(spriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 - 64 - 8) + num8)), position, Color.White * 0.75f, 0.0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((double)((y + 1) * 64) / 10000.0 + 9.99999974737875E-06 + (double)instance.tileLocation.X / 10000.0 + ((int)(instance.parentSheetIndex) == 105 ? 0.00150000001303852 : 0.0)));
            if (!(instance.heldObject.Value is ColoredObject))
                return;
            spriteBatch.Draw(spriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32), (float)(y * 64 - 64 - 8) + num8)), position, (instance.heldObject.Value as ColoredObject).color.Value * 0.75f, 0.0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((double)((y + 1) * 64) / 10000.0 + 9.99999974737875E-06 + (double)instance.tileLocation.X / 10000.0 + ((int)(instance.parentSheetIndex) == 105 ? 0.00150000001303852 : 9.99999974737875E-06)));
        }
    }
}
