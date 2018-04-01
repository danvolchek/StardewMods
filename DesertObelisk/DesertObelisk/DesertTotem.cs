using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace DesertObelisk
{
    class DesertTotem : SObject
    {
        public static Texture2D totemTexture;

        public DesertTotem(int initalStack) : base(926374, initalStack)
        {

        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            if (f.ActiveObject.bigCraftable)
            {
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, objectPosition, new Microsoft.Xna.Framework.Rectangle?(SObject.getSourceRectForBigCraftable(f.ActiveObject.ParentSheetIndex)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
            }
            else
            {
                spriteBatch.Draw(totemTexture, objectPosition, null, Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
                if (f.ActiveObject == null || !f.ActiveObject.Name.Contains("="))
                    return;
                spriteBatch.Draw(Game1.objectSpriteSheet, objectPosition + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Microsoft.Xna.Framework.Rectangle?(Game1.currentLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex)), Color.White, 0.0f, new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), (float)Game1.pixelZoom + Math.Abs(Game1.starCropShimmerPause) / 8f, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
                if ((double)Math.Abs(Game1.starCropShimmerPause) <= 0.0500000007450581 && Game1.random.NextDouble() < 0.97)
                    return;
                Game1.starCropShimmerPause += 0.04f;
                if ((double)Game1.starCropShimmerPause < 0.800000011920929)
                    return;
                Game1.starCropShimmerPause = -0.8f;
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            spriteBatch.Draw(Game1.shadowTexture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize * 3 / 4)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * 0.5f, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            spriteBatch.Draw(totemTexture, location + new Vector2((float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize), (float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize)), null, Color.White * transparency, 0.0f, new Vector2(8f, 8f) * scaleSize, (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);
            if (drawStackNumber && this.maximumStackSize() > 1 && ((double)scaleSize > 0.3 && this.Stack != int.MaxValue) && this.Stack > 1)
                Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
            if (drawStackNumber && this.quality > 0)
            {
                float num = this.quality < 4 ? 0.0f : (float)((Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, (float)(Game1.tileSize - 12) + num), new Microsoft.Xna.Framework.Rectangle?(this.quality < 4 ? new Microsoft.Xna.Framework.Rectangle(338 + (this.quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8)), Color.White * transparency, 0.0f, new Vector2(4f, 4f), (float)(3.0 * (double)scaleSize * (1.0 + (double)num)), SpriteEffects.None, layerDepth);
            }
            if (this.category == -22 && (double)this.scale.Y < 1.0)
                spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)location.X, (int)((double)location.Y + (double)(Game1.tileSize - 2 * Game1.pixelZoom) * (double)scaleSize), (int)((double)Game1.tileSize * (double)scaleSize * (double)this.scale.Y), (int)((double)(2 * Game1.pixelZoom) * (double)scaleSize)), Utility.getRedToGreenLerpColor(this.scale.Y));

        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (!Game1.eventUp || Game1.CurrentEvent != null && !Game1.CurrentEvent.isTileWalkedOn(x, y))
            {
                Microsoft.Xna.Framework.Rectangle boundingBox;
                if (this.fragility != 2)
                {
                    SpriteBatch spriteBatch1 = spriteBatch;
                    Texture2D shadowTexture = Game1.shadowTexture;
                    Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize + Game1.tileSize / 2), (float)(y * Game1.tileSize + Game1.tileSize * 4 / 5 + Game1.pixelZoom)));
                    Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds);
                    Color color = Color.White * alpha;
                    double num1 = 0.0;
                    Vector2 origin = new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y);
                    double num2 = 4.0;
                    int num3 = 0;
                    boundingBox = this.getBoundingBox(new Vector2((float)x, (float)y));
                    double num4 = (double)boundingBox.Bottom / 15000.0;
                    spriteBatch1.Draw(shadowTexture, local, sourceRectangle, color, (float)num1, origin, (float)num2, (SpriteEffects)num3, (float)num4);
                }
                SpriteBatch spriteBatch2 = spriteBatch;
                Texture2D objectSpriteSheet = totemTexture;
                Vector2 local1 = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize + Game1.tileSize / 2 + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)), (float)(y * Game1.tileSize + Game1.tileSize / 2 + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0))));
                Microsoft.Xna.Framework.Rectangle? sourceRectangle1 = null;
                Color color1 = Color.White * alpha;
                double num5 = 0.0;
                Vector2 origin1 = new Vector2(8f, 8f);
                Vector2 scale1 = this.scale;
                double num6 = (double)this.scale.Y > 1.0 ? (double)this.getScale().Y : (double)Game1.pixelZoom;
                int num7 = this.flipped ? 1 : 0;
                int num8;
                if (!this.isPassable())
                {
                    boundingBox = this.getBoundingBox(new Vector2((float)x, (float)y));
                    num8 = boundingBox.Bottom;
                }
                else
                {
                    boundingBox = this.getBoundingBox(new Vector2((float)x, (float)y));
                    num8 = boundingBox.Top;
                }
                double num9 = (double)num8 / 10000.0;
                spriteBatch2.Draw(objectSpriteSheet, local1, sourceRectangle1, color1, (float)num5, origin1, (float)num6, (SpriteEffects)num7, (float)num9);
            }
           
        }
    }
}
