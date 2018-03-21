using static Pong.PongGame;

namespace Pong.Game
{
    class CollideInfo
    {
        public Orientation orientation;
        public double collidePercentage;

        public CollideInfo(Orientation o, double c)
        {
            orientation = o;
            collidePercentage = c;
        }
    }
}
