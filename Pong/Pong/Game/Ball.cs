using Microsoft.Xna.Framework.Graphics;
using Pong.Game.Interfaces;
using StardewValley;
using System;

namespace Pong.Game
{
    class Ball : Collider, IReactiveCollideable, IResetable
    {
        private int xVelocity;
        private int yVelocity;

        private Random rand;
        public Ball() : base(false)
        {
            width = height = Game1.tileSize;
            rand = new Random();
            Reset();
        }

        public void Reset()
        {
            xPos = (PongGame.GetScreenWidth() - this.width) / 2;
            yPos = (PongGame.GetScreenHeight() - this.height) / 2;
            xVelocity = (rand.NextDouble() < 0.5 ? 1 : -1) * 4;
            yVelocity = (rand.NextDouble() < 0.5 ? 1 : -1) * 8;
        }

        public void CollideWith(INonReactiveCollideable other)
        {
            CollideInfo info = other.GetCollideInfo(this);

            if (info.orientation == PongGame.Orientation.HORIZONTAL)
            {
                yVelocity *= -1;

                if (info.collidePercentage >= 0)
                {
                    xVelocity = (int)(30 * info.collidePercentage - 15);
                }
            }
            else
                xVelocity *= -1;
        }

        public override void Update()
        {
            xPos += xVelocity;
            yPos += yVelocity;
        }
    }
}
