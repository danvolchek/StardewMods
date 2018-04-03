using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;

namespace DesertObelisk
{
    class DesertObelisk : Building
    {
        private int desertWarpX;
        public DesertObelisk(BluePrint blueprint, Vector2 tileLocation, int desertWarpX) : base(blueprint, tileLocation)
        {
            this.daysOfConstructionLeft = 0;
            this.desertWarpX = desertWarpX;
        }

        public override bool doAction(Vector2 tileLocation, StardewValley.Farmer who)
        {
            if (who.IsMainPlayer && !this.isTilePassable(tileLocation))
            {
                for (int index = 0; index < 12; ++index)
                    who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)who.position.X - Game1.tileSize * 4, (int)who.position.X + Game1.tileSize * 3), (float)Game1.random.Next((int)who.position.Y - Game1.tileSize * 4, (int)who.position.Y + Game1.tileSize * 3)), false, Game1.random.NextDouble() < 0.5));
                Game1.playSound("wand");
                Game1.displayFarmer = false;
                Game1.player.freezePause = 1000;
                Game1.flashAlpha = 1f;
                DelayedAction.fadeAfterDelay(new Game1.afterFadeFunction(this.WarpToDesert), 1000);
                new Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, Game1.tileSize, Game1.tileSize).Inflate(Game1.tileSize * 3, Game1.tileSize * 3);
                int num1 = 0;
                for (int index = who.getTileX() + 8; index >= who.getTileX() - 8; --index)
                {
                    List<TemporaryAnimatedSprite> temporarySprites = who.currentLocation.temporarySprites;
                    TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(6, new Vector2((float)index, (float)who.getTileY()) * (float)Game1.tileSize, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0);
                    temporaryAnimatedSprite.layerDepth = 1f;
                    int num2 = num1 * 25;
                    temporaryAnimatedSprite.delayBeforeAnimationStart = num2;
                    Vector2 vector2 = new Vector2(-0.25f, 0.0f);
                    temporaryAnimatedSprite.motion = vector2;
                    temporarySprites.Add(temporaryAnimatedSprite);
                    ++num1;
                }
            }
            return false;
        }

        private void WarpToDesert()
        {
            Game1.warpFarmer("Desert", desertWarpX, 43, 2);
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;

            GameEvents.UpdateTick += this.CheckForWarpOver;
        }

        private void CheckForWarpOver(object sender, EventArgs e)
        {
            if (!Game1.eventUp && Game1.currentLocation.Name == "Desert")
            {
                Game1.player.position.X += 0.5f * Game1.tileSize;
                GameEvents.UpdateTick -= this.CheckForWarpOver;
            }
        }
    }
}
