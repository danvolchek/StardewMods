using Microsoft.Xna.Framework;
using Pong.Game.Interfaces;
using StardewValley;
using System;

namespace Pong.Game
{
    internal class Paddle : Collider, INonReactiveCollideable, IResetable
    {
        private readonly bool isPlayerControlled;

        private int intendedPosition;
        private readonly int movementAmount = 9;

        private int fuzz = 0;
        private readonly Random rand;

        public Paddle(bool isPlayerControlled) : base(true)
        {
            this.Width = Game1.tileSize * 5;
            this.Height = Game1.tileSize / 2;
            this.rand = new Random();

            this.ResetPos();

            this.intendedPosition = 0;
            this.isPlayerControlled = isPlayerControlled;
            if (!isPlayerControlled) this.movementAmount--;

            this.fuzz = (int)(Game1.random.NextDouble() * this.Width - this.Width / 2);
        }

        public void Move(bool left)
        {
            if (left && this.XPos < this.movementAmount)
                return;
            if (!left && this.XPos > PongGame.GetScreenWidth() - this.Width - this.movementAmount)
                return;
            this.XPos += (left ? -1 : 1) * this.movementAmount;
        }

        public void ReceiveIntendedPosition(int pos)
        {
            this.intendedPosition = pos + (this.isPlayerControlled ? 0 : this.fuzz);
        }

        public override void Update()
        {
            if (Math.Abs(this.intendedPosition - (this.XPos + this.Width / 2)) > this.movementAmount)
            {
                if (this.intendedPosition < this.XPos + this.Width / 2)
                    this.Move(true);
                else
                    this.Move(false);
            }

        }

        public CollideInfo GetCollideInfo(IReactiveCollideable other)
        {
            this.fuzz = (int)(this.rand.NextDouble() * this.Width - this.Width / 2);

            Rectangle otherPos = other.GetBoundingBox();
            return new CollideInfo(PongGame.Orientation.Horizontal, Math.Max(0, (otherPos.X + otherPos.Width / 2.0 - this.XPos) / this.Width));
        }

        private void ResetPos()
        {
            this.XPos = (PongGame.GetScreenWidth() - this.Width) / 2;
            this.YPos = this.isPlayerControlled ? PongGame.GetScreenHeight() - this.Height : 0;
        }

        public void Resize()
        {
            this.ResetPos();
        }

        public void Reset()
        {
            this.ResetPos();
        }
    }
}
