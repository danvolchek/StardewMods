using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace CopyInviteCode
{
    /// <summary>A dummy item used to draw a <see cref="HUDMessage"/>.</summary>
    internal class ClipboardItem : Item
    {
        private readonly Texture2D clipboardTexture;

        public ClipboardItem(Texture2D texture)
        {
            this.clipboardTexture = texture;
        }

        public override string DisplayName
        {
            get => "Copied invite code to clipboard!";
            set { }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency,
            float layerDepth,
            bool drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(this.clipboardTexture, new Rectangle((int)(location.X + 8 * scaleSize), (int)(location.Y + 8 * scaleSize), 64, 64), new Rectangle(0, 0, 64, 64), color * transparency, 0.0f, new Vector2(8f, 8f) * scaleSize, SpriteEffects.None,
                layerDepth);
        }

        public override int Stack
        {
            get => -1;
            set { }
        }

        public override int maximumStackSize()
        {
            return -1;
        }

        public override int getStack()
        {
            return -1;
        }

        public override int addToStack(int amount)
        {
            return amount;
        }

        public override string getDescription()
        {
            return "";
        }

        public override bool isPlaceable()
        {
            return false;
        }

        public override Item getOne()
        {
            return new ClipboardItem(this.clipboardTexture);
        }
    }
}
