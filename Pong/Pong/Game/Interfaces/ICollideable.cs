using Microsoft.Xna.Framework;

namespace Pong.Game.Interfaces
{
    internal interface ICollideable : IUpdateable
    {
        Rectangle GetBoundingBox();
    }
}