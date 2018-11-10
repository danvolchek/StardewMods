using Pong.Framework.Enums;
using Pong.Framework.Interfaces.Game;

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
                    this.XPos = -10;
                    this.YPos = 0;
                    this.Width = 10;
                    this.Height = GameMenu.ScreenHeight;

                    orientation = Orientation.Vertical;
                    break;

                case Side.Right:

                    this.XPos = GameMenu.ScreenWidth;
                    this.YPos = 0;
                    this.Width = 10;
                    this.Height = GameMenu.ScreenHeight;

                    orientation = Orientation.Vertical;
                    break;
                case Side.Top:

                    this.XPos = 0;
                    this.YPos = -10;
                    this.Width = GameMenu.ScreenWidth;
                    this.Height = 10;

                    orientation = Orientation.Horizontal;
                    break;
                default:
                case Side.Bottom:

                    this.XPos = 0;
                    this.YPos = GameMenu.ScreenHeight;
                    this.Width = GameMenu.ScreenWidth;
                    this.Height = 10;

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
