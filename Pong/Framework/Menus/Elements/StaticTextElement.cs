using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Extensions;
using StardewValley.BellsAndWhistles;
using IDrawable = Pong.Framework.Common.IDrawable;

namespace Pong.Framework.Menus.Elements
{
    internal class StaticTextElement : IDrawable, IHighlightable, IClickable
    {
        private string text;

        protected string Text
        {
            get => this.text;
            set { this.text = value; this.UpdateBounds(); }
        }

        public delegate void ClickFunc();

        private readonly int xPos;
        private readonly int yPos;
        private readonly bool centered;
        private readonly int color;
        private readonly bool neverHighlight;
        private readonly ClickFunc onClick;

        public Rectangle Bounds { get; private set; }

        public void Clicked()
        {
            this.onClick?.Invoke();
        }

        public static int HighlightWidth { get; } = 10;
        public bool Highlighted { get; set; }

        public StaticTextElement(string text, int x, int y, bool centered = true, bool neverHighlight = false, ClickFunc onClick = null, int color = SpriteText.color_White)
        {
            this.xPos = x;
            this.yPos = y;
            this.centered = centered;
            this.color = color;
            this.neverHighlight = neverHighlight;
            this.onClick = onClick;
            this.Text = text;
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

            if (!this.neverHighlight && this.Highlighted)
            {
                this.DrawBorder(b);
            }
        }

        private void UpdateBounds()
        {
            int width = SpriteText.getWidthOfString(this.Text);
            int height = SpriteText.getHeightOfString(this.Text);

            this.Bounds = this.centered ? new Rectangle(this.xPos - width / 2, this.yPos, width, height) : new Rectangle(this.xPos, this.yPos, width, height);
        }
    }
}
