using static Pong.PongGame;

namespace Pong.Game
{
    class Wall : Collider, INonReactiveCollideable
    {

        private CollideInfo collideInfo;

        public Side side;

        public Wall(Side side) : base(true)
        {
            this.side = side;

            collideInfo = new CollideInfo(SetPosition(), -1);
        }

        private Orientation SetPosition()
        {
            Orientation orientation;
            switch (side)
            {
                case Side.LEFT:
                    xPos = -10;
                    yPos = 0;
                    width = 10;
                    height = PongGame.GetScreenHeight();

                    orientation = Orientation.VERTICAL;
                    break;

                case Side.RIGHT:

                    xPos = GetScreenWidth();
                    yPos = 0;
                    width = 10;
                    height = PongGame.GetScreenHeight();

                    orientation = Orientation.VERTICAL;
                    break;
                case Side.TOP:

                    xPos = 0;
                    yPos = -10;
                    width = PongGame.GetScreenWidth();
                    height = 10;

                    orientation = Orientation.HORIZONTAL;
                    break;
                default:
                case Side.BOTTOM:

                    xPos = 0;
                    yPos = PongGame.GetScreenHeight();
                    width = PongGame.GetScreenWidth();
                    height = 10;

                    orientation = Orientation.HORIZONTAL;
                    break;
            }
            return orientation;
        }

        public CollideInfo GetCollideInfo(IReactiveCollideable other)
        {
            return collideInfo;
        }

        public void Resize()
        {
            SetPosition();
        }
    }
}
