using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Common;
using System.Collections.Generic;

namespace Pong.Framework.Menus.Elements
{
    internal class ElementContainer : IDrawable
    {
        public IList<IDrawable> Elements { get; } = new List<IDrawable>();

        public void Draw(SpriteBatch b)
        {
            foreach (IDrawable element in this.Elements)
                element.Draw(b);
        }
    }
}
