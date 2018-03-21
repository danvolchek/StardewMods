using Microsoft.Xna.Framework;
using Pong.Game.Interfaces;
using StardewValley;
using System;

namespace Pong.Game
{
    class Paddle : Collider, INonReactiveCollideable, IResetable
    {
        private bool isPlayerControlled;

        private int intendedPosition;
        private int movementAmount = 9;

        private int fuzz = 0;
        private Random rand;

        public Paddle(bool isPlayerControlled) : base(true)
        {
            width = Game1.tileSize * 5;
            height = Game1.tileSize / 2;
            rand = new Random();

            ResetPos();

            intendedPosition = 0;
            this.isPlayerControlled = isPlayerControlled;
            if (!isPlayerControlled)
               movementAmount--;

            fuzz = (int)(Game1.random.NextDouble() * width - width / 2);
        }

        public void Move(bool left)
        {
            if (left && xPos < movementAmount)
                return;
            if (!left && xPos > PongGame.GetScreenWidth() - width - movementAmount)
                return;
            xPos += (left ? -1 : 1) * movementAmount;
        }

        public void ReceiveIntendedPosition(int pos)
        {
            intendedPosition = pos + (isPlayerControlled ? 0 : fuzz);
        }

        public override void Update()
        {
            if (Math.Abs(intendedPosition - (xPos + width / 2)) > movementAmount)
            {
                if (intendedPosition < (xPos + width / 2))
                {
                    Move(true);
                }
                else
                {
                    Move(false);
                }
            }

        }

        public CollideInfo GetCollideInfo(IReactiveCollideable other)
        {
            fuzz = (int)(rand.NextDouble() * width - width / 2);

            Rectangle otherPos = other.GetBoundingBox();
            return new CollideInfo(PongGame.Orientation.HORIZONTAL, Math.Max(0, (otherPos.X + otherPos.Width / 2.0 - xPos) / width));
        }

        private void ResetPos()
        {
            xPos = (PongGame.GetScreenWidth() - this.width) / 2;
            yPos = isPlayerControlled ? PongGame.GetScreenHeight() - this.height : 0;
        }

        public void Resize()
        {
            ResetPos();
        }

        public void Reset()
        {
            ResetPos();
        }
    }
}
