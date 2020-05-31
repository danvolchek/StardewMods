using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FloweryTools.Framework.Flowerers
{
    internal class FlowerHelper
    {
        public Random rand = new Random();
        private Multiplayer multiplayer;
        private bool localOnly;

        public FlowerHelper(Multiplayer multiplayer, bool localOnly)
        {
            this.multiplayer = multiplayer;
            this.localOnly = localOnly;
        }

        public void AddFlower(Vector2 offset, Vector2 motion, GameLocation location)
        {
            TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("Maps/springobjects", this.FlowerToRect(GetFlower()), 999999f, 1, 0, Game1.player.Position + offset, false, false, 1f, 0.001f + GetAlphaFadeOffset(), Color.White, GetScale(), 0f, 0f, GetRotationChange())
            {
               motion = motion,
               timeBasedMotion = true
            };
            
            if (localOnly)
                location.TemporarySprites.Add(sprite);
            else
                this.multiplayer.broadcastSprites(location, sprite);
        }

        public bool MatchHelper(float timer, int maxFrame, IList<FarmerSprite.AnimationFrame> actual, IList<Tuple<int, int>> target)
        {
            if (Game1.currentGameTime == null || actual == null || timer > Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds  * maxFrame + 0.1f || actual.Count != target.Count)
                return false;

            for (int i = 0; i < actual.Count; i++)
                if (actual[i].frame != target[i].Item1 || (target[i].Item2 != -1 && actual[i].milliseconds != target[i].Item2))
                    return false;

            return true;
        }

        private float GetScale()
        {
            return 3 + 3 * (float)(this.rand.NextDouble() - 0.3);
        }

        private float GetAlphaFadeOffset()
        {
            return (float)(this.rand.NextDouble() - 0.5) / 1000;
        }

        private Flower GetFlower()
        {
            switch (this.rand.Next(4))
            {
                default:
                case 0:
                    return Flower.Tulip;
                case 1:
                    return Flower.SummerSpangle;
                case 2:
                    return Flower.FairyRose;
                case 3:
                    return Flower.BlueJazz;
            }
        }

        private float GetRotationChange()
        {
            return (float)(this.rand.NextDouble() / 10 - 0.05);
        }

        private Rectangle FlowerToRect(Flower flower)
        {
            switch(flower)
            {
                default:
                case Flower.Tulip:
                    return new Rectangle(256 -16, 384, 16, 16);
                case Flower.SummerSpangle:
                    return new Rectangle(256 + 16 * 2 - 16, 384, 16, 16);
                case Flower.FairyRose:
                    return new Rectangle(256 + 16 * 4 - 16, 384, 16, 16);
                case Flower.BlueJazz:
                    return new Rectangle(256 + 16 * 6 - 16, 384, 16, 16);
            }
        }
        public Vector2 ApplyJitter(Vector2 input, float scale)
        {
            input.X += scale * (float)(this.rand.NextDouble() - 0.5);
            input.Y += scale * (float)(this.rand.NextDouble() - 0.5);
            return input;
        }

        public Vector2 GetOffset(int phase)
        {
            int startAngle = -1;
            int dir = Game1.player.FacingDirection;
            switch (dir)
            {
                case 0:
                    startAngle = 45 * 5;
                    break;
                case 1:
                    startAngle = 45 * 7;
                    break;
                case 2:
                    startAngle = 45;
                    break;
                case 3:
                    startAngle = 45 * 5;
                    break;
            }

            int mul = -1;
            if (dir == 1 || dir == 0 || dir == 2)
            {
                mul = 1;
            }
            startAngle += mul * (90 / 4) * phase;

            startAngle %= 360;

            double angleRad = startAngle * (Math.PI / 180);

            return 50 * new Vector2((float)Math.Cos(angleRad), (float)Math.Sin(angleRad));
        }

        public Vector2 GetExplosionMotion()
        {
            return new Vector2((float)(this.rand.NextDouble() / 2 - 0.25), (float)(this.rand.NextDouble() / 2 - 0.25));
        }

        public Vector2 GetDefaultMotion()
        {
            switch (Game1.player.FacingDirection)
            {
                default:
                case 0:
                    return new Vector2(0, -0.3f);
                case 1:
                    return new Vector2(0.3f, 0);
                case 2:
                    return new Vector2(0, 0.3f);
                case 3:
                    return new Vector2(-0.3f, 0);
            }
        }
    }
}
