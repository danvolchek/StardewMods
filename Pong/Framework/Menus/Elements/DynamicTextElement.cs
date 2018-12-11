using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace Pong.Framework.Menus.Elements
{
    internal class DynamicTextElement : StaticTextElement
    {
        private readonly TextFunc getText;

        public DynamicTextElement(TextFunc getText, int x, int y, bool centered = false, bool neverHighlight = false, ClickFunc onClick = null, int color = SpriteText.color_White) : base("", x, y, centered, neverHighlight, onClick, color)
        {
            this.getText = getText;
        }

        public override void Draw(SpriteBatch b)
        {
            this.Text = this.getText();
            base.Draw(b);
        }

        public delegate string TextFunc();
    }
}
