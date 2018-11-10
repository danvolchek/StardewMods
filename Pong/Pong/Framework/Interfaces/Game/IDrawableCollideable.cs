using Microsoft.Xna.Framework;
using IDrawable = Pong.Framework.Common.IDrawable;
using IUpdateable = Pong.Framework.Common.IUpdateable;

namespace Pong.Framework.Interfaces.Game
{
    internal interface IDrawableCollideable : IUpdateable, IDrawable
    {
        Rectangle GetBoundingBox();
    }
}