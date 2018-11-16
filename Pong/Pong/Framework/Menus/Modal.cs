using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Common;
using Pong.Framework.Enums;
using Pong.Framework.Extensions;
using Pong.Framework.Menus.Elements;
using StardewModdingAPI.Events;
using IDrawable = Pong.Framework.Common.IDrawable;

namespace Pong.Framework.Menus
{
    class Modal : IDrawable, IBoundable, IInputable
    {
        public Modal(string text, ModalButtonType buttonType)
        {
            this.Bounds = new Rectangle(50, 50, 400, 400);
        }

        public void Draw(SpriteBatch b)
        {
            b.Draw(AssetManager.SquareTexture, this.Bounds, null, Color.Black);
            this.DrawBorder(b);
        }

        public Rectangle Bounds { get; }
        public bool ButtonPressed(EventArgsInput e)
        {
            throw new System.NotImplementedException();
        }

        public void MouseStateChanged(EventArgsMouseStateChanged e)
        {
            throw new System.NotImplementedException();
        }
    }
}
