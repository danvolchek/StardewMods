using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace DesertObelisk
{
    internal class DesertObelisk : Building
    {
        private readonly int desertWarpX;

        /// <summary>Whether the player is currently being warped.</summary>
        private bool IsWarping;

        //Parameterless constructor for when its synced to other players
        public DesertObelisk()
        {
            this.desertWarpX = ModEntry.DesertWarpX;
            this.daysOfConstructionLeft.Value = 0;
        }

        public DesertObelisk(BluePrint blueprint, Vector2 tileLocation, int desertWarpX) : base(blueprint, tileLocation)
        {
            this.daysOfConstructionLeft.Value = 0;
            this.desertWarpX = desertWarpX;
        }

        public override bool doAction(Vector2 tileLocation, Farmer who)
        {
            if (who.IsLocalPlayer && !this.isTilePassable(tileLocation))
            {
                for (var index = 0; index < 12; ++index)
                    who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75),
                        6, 1,
                        new Vector2(
                            Game1.random.Next((int)who.position.X - Game1.tileSize * 4,
                                (int)who.position.X + Game1.tileSize * 3),
                            Game1.random.Next((int)who.position.Y - Game1.tileSize * 4,
                                (int)who.position.Y + Game1.tileSize * 3)), false, Game1.random.NextDouble() < 0.5));
                who.currentLocation.playSound("wand");
                Game1.displayFarmer = false;
                Game1.player.freezePause = 1000;
                Game1.flashAlpha = 1f;
                DelayedAction.fadeAfterDelay(this.WarpToDesert, 1000);
                new Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, Game1.tileSize, Game1.tileSize).Inflate(
                    Game1.tileSize * 3, Game1.tileSize * 3);
                var num = 0;
                for (var index = who.getTileX() + 8; index >= who.getTileX() - 8; --index)
                {
                    who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6,
                        new Vector2(index, who.getTileY()) * 64f, Color.White, 8, false, 50f, 0, -1, -1f, -1, 0)
                    {
                        layerDepth = 1f,
                        delayBeforeAnimationStart = num * 25,
                        motion = new Vector2(-0.25f, 0.0f)
                    });
                    ++num;
                }
            }

            return false;
        }

        public override void Update(GameTime time)
        {
            // check for completed warp
            if (this.IsWarping && !Game1.eventUp && Game1.currentLocation.Name == "Desert")
            {
                Game1.player.position.X += 0.5f * Game1.tileSize;
                this.IsWarping = false;
            }

            base.Update(time);
        }

        private void WarpToDesert()
        {
            Game1.warpFarmer("Desert", this.desertWarpX, 43, 2);
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
            this.IsWarping = true;
        }
    }
}
