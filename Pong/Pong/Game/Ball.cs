using StardewValley;
using System;
using Pong.Game.Framework;
using Pong.Game.Framework.Enums;

namespace Pong.Game
{
    internal class Ball : Collider, IReactiveDrawableCollideable, IResetable
    {
        private readonly Random rand;
        private int xVelocity;
        private int yVelocity;

        public Ball() : base(false)
        {
            this.Width = this.Height = Game1.tileSize;
            this.rand = new Random();
            this.Reset();
        }

        public void CollideWith(INonReactiveDrawableCollideable other)
        {
            CollideInfo info = other.GetCollideInfo(this);

            if (info.Orientation == Orientation.Horizontal)
            {
                this.yVelocity *= -1;

                if (info.CollidePercentage >= 0) this.xVelocity = (int)(30 * info.CollidePercentage - 15);
            }
            else
            {
                this.xVelocity *= -1;
            }
        }

        public void Reset()
        {
            this.XPos = (PongGame.ScreenWidth- this.Width) / 2;
            this.YPos = (PongGame.ScreenHeight- this.Height) / 2;
            this.xVelocity = (this.rand.NextDouble() < 0.5 ? 1 : -1) * 4;
            this.yVelocity = (this.rand.NextDouble() < 0.5 ? 1 : -1) * 8;
        }

        public override void Update()
        {
            this.XPos += this.xVelocity;
            this.YPos += this.yVelocity;
        }
    }
}