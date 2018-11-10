using Microsoft.Xna.Framework;

namespace Pong.Game.Framework
{
    internal interface IDrawableCollideable : IUpdateable, IDrawable
    {
        Rectangle GetBoundingBox();
    }
}