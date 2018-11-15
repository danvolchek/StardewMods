using Pong.Framework.Enums;
using Pong.Framework.Game;
using Pong.Framework.Menus;

namespace Pong.Game
{
    internal class Wall : Collider, INonReactiveDrawableCollideable
    {
        private readonly CollideInfo collideInfo;
        public Side Side { get; }

        public Wall(Side side) : base(true)
        {
            this.Side = side;

            this.collideInfo = new CollideInfo(this.SetPosition(), -1);
        }

        private Orientation SetPosition()
        {
            Orientation orientation;
            switch (this.Side)
            {
                case Side.Left:
                    this.Width = 10;
                    this.Height = Menu.ScreenHeight;
                    this.XPos = -10;
                    this.YPos = 0;

                    orientation = Orientation.Vertical;
                    break;

                case Side.Right:
                    this.Width = 10;
                    this.Height = Menu.ScreenHeight;
                    this.XPos = Menu.ScreenWidth;
                    this.YPos = 0;

                    orientation = Orientation.Vertical;
                    break;
                case Side.Top:
                    this.Width = Menu.ScreenWidth;
                    this.Height = 10;
                    this.XPos = 0;
                    this.YPos = -10;

                    orientation = Orientation.Horizontal;
                    break;
                default:
                case Side.Bottom:
                    this.Width = Menu.ScreenWidth;
                    this.Height = 10;
                    this.XPos = 0;
                    this.YPos = Menu.ScreenHeight;

                    orientation = Orientation.Horizontal;
                    break;
            }
            return orientation;
        }

        public CollideInfo GetCollideInfo(IReactiveDrawableCollideable other)
        {
            return this.collideInfo;
        }

        public void Resize()
        {
            this.SetPosition();
        }
    }
}
