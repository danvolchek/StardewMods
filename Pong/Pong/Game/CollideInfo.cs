using Pong.Game.Framework;
using Pong.Game.Framework.Enums;

namespace Pong.Game
{
    internal class CollideInfo
    {
        public Orientation Orientation { get; }
        public double CollidePercentage { get; }

        public CollideInfo(Orientation o, double c)
        {
            this.Orientation = o;
            this.CollidePercentage = c;
        }
    }
}
