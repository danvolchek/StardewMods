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
    internal class Modal : IDrawable, IBoundable, IInputable
    {
        public Rectangle Bounds { get; }

        public Modal(string text, ModalButtonType buttonType)
        {
            this.Bounds = new Rectangle(50, 50, 400, 400);
        }

        public void Draw(SpriteBatch b)
        {
            b.Draw(AssetManager.SquareTexture, this.Bounds, null, Color.Black);
            this.DrawBorder(b);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event arguments.</param>
        public bool OnButtonPressed(ButtonPressedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnCursorMoved(CursorMovedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
