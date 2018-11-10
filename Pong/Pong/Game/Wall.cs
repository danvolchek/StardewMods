using Pong.Game.Interfaces;
using static Pong.Game.PongGame;

namespace Pong.Game
{
    internal class Wall : Collider, INonReactiveCollideable
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
                    this.Height = GetScreenHeight();

                    orientation = Orientation.Vertical;
                    break;

                case Side.Right:

                    this.XPos = GetScreenWidth();
                    this.YPos = 0;
                    this.Width = 10;
                    this.Height = GetScreenHeight();

                    orientation = Orientation.Vertical;
                    break;
                case Side.Top:

                    this.XPos = 0;
                    this.YPos = -10;
                    this.Width = GetScreenWidth();
                    this.Height = 10;

                    orientation = Orientation.Horizontal;
                    break;
                default:
                case Side.Bottom:

                    this.XPos = 0;
                    this.YPos = GetScreenHeight();
                    this.Width = GetScreenWidth();
                    this.Height = 10;

                    orientation = Orientation.Horizontal;
                    break;
            }
            return orientation;
        }

        public CollideInfo GetCollideInfo(IReactiveCollideable other)
        {
            return this.collideInfo;
        }

        public void Resize()
        {
            this.SetPosition();
        }
    }
}
