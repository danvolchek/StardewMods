using System;

namespace Pong.Game.Framework.Controllers
{
    internal abstract class PaddleController : IUpdateable
    {
        private readonly int movementAmount = 9;
        protected int IntendedPosition = 0;

        public int GetMovement(int xPos, int width)
        {
            if (Math.Abs(this.IntendedPosition - (xPos + width / 2)) > this.movementAmount)
            {
                if (this.IntendedPosition < xPos + width / 2)
                    return this.Move(true, xPos, width);
                else
                    return this.Move(false, xPos, width);
            }

            return 0;
        }

        public abstract void Update();

        private int Move(bool left, int xPos, int width)
        {
            if (left && xPos < this.movementAmount)
                return 0;
            if (!left && xPos > PongGame.ScreenWidth - width - this.movementAmount)
                return 0;
            return (left ? -1 : 1) * this.movementAmount;
        }
    }
}