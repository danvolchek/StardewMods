using Pong.Framework.Menus.Elements;
using IDrawable = Pong.Framework.Common.IDrawable;
using IUpdateable = Pong.Framework.Common.IUpdateable;

namespace Pong.Framework.Game
{
    internal interface IDrawableCollideable : IUpdateable, IDrawable, IBoundable
    {
    }
}
