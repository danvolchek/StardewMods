using Microsoft.Xna.Framework;
using Pong.Framework.Enums;
using Pong.Framework.Menus;
using Pong.Game.Controllers;
using StardewValley;
using System;
using Pong.Framework.Game;

namespace Pong.Game
{
    internal class Paddle : Collider, INonReactiveDrawableCollideable, IResetable
    {
        private readonly PaddleController controller;

        private readonly Side side;

        public Paddle(PaddleController controller, Side side) : base(true)
        {
            this.Width = Game1.tileSize * 5;
            this.Height = Game1.tileSize / 2;

            this.controller = controller;
            this.side = side;

            this.ResetPos();
        }

        public override void Update()
        {
            this.controller.Update();
            this.XPos += this.controller.GetMovement(this.XPos, this.Width);
        }

        public CollideInfo GetCollideInfo(IReactiveDrawableCollideable other)
        {
            Rectangle otherPos = other.GetBoundingBox();
            return new CollideInfo(Orientation.Horizontal, Math.Max(0, (otherPos.X + otherPos.Width / 2.0 - this.XPos) / this.Width));
        }

        private void ResetPos()
        {
            this.XPos = (Menu.ScreenWidth - this.Width) / 2;
            this.YPos = this.side == Side.Bottom ? Menu.ScreenHeight - this.Height : 0;
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
