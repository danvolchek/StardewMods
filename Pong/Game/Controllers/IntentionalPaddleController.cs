﻿using System;
using static Pong.Framework.Menus.Menu;

namespace Pong.Game.Controllers
{
    internal abstract class IntentionalPaddleController : IPaddleController
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
            if (!left && xPos > ScreenWidth - width - this.movementAmount)
                return 0;
            return (left ? -1 : 1) * this.movementAmount;
        }
    }
}