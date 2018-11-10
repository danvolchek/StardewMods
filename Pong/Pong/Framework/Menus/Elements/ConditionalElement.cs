using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Common;

namespace Pong.Framework.Menus.Elements
{
    internal class ConditionalElement : IDrawable
    {
        private readonly IDrawable element;
        private readonly DrawCondition condition;

        public ConditionalElement(IDrawable element, DrawCondition condition)
        {
            this.element = element;
            this.condition = condition;
        }
        public void Draw(SpriteBatch b)
        {
            if(this.condition())
                this.element.Draw(b);
        }

        public delegate bool DrawCondition();
    }
}
