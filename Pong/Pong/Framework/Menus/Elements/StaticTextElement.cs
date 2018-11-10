using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Common;
using StardewValley.BellsAndWhistles;

namespace Pong.Framework.Menus.Elements
{
    internal class StaticTextElement : IDrawable
    {
        protected string Text;
        private readonly int xPos;
        private readonly int yPos;
        private readonly bool centered;
        private readonly int color;

        public StaticTextElement(string text, int x, int y, bool centered = true, int color = SpriteText.color_White)
        {
            this.Text = text;
            this.xPos = x;
            this.yPos = y;
            this.centered = centered;
            this.color = color;

        }

        public virtual void Draw(SpriteBatch b)
        {
            if (this.centered)
            {
                SpriteText.drawStringHorizontallyCenteredAt(b, this.Text, this.xPos,
                    this.yPos, 999999, -1, 999999, 1f, 0.88f, false,
                    this.color);
            }
            else
            {
                SpriteText.drawString(b, this.Text, this.xPos, this.yPos, 999999, -1, 999999, 1f,
                    0.88f, false, -1, "", SpriteText.color_White);
            }
        }
    }
}
