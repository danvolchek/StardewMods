using Microsoft.Xna.Framework;

namespace Pong.Framework.Interfaces
{
    internal interface IDrawableCollideable : IUpdateable, IDrawable
    {
        Rectangle GetBoundingBox();
    }
}