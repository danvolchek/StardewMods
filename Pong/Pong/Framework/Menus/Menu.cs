using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Menus;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using IDrawable = Pong.Framework.Common.IDrawable;

namespace Pong.Framework.Menus
{
    internal abstract class Menu : IMenu
    {
        private readonly List<IDrawable> drawables = new List<IDrawable>();

        protected void InitDrawables()
        {
            this.drawables.AddRange(this.GetDrawables());
        }

        protected virtual IEnumerable<IDrawable> GetDrawables()
        {
            yield break;
        }

        public event EventHandler<SwitchMenuEventArgs> SwitchToNewMenu;

        public virtual bool ButtonPressed(EventArgsInput e)
        {
            return false;
        }

        public virtual void MouseStateChanged(EventArgsMouseStateChanged e)
        {

        }

        public void Draw(SpriteBatch b)
        {
            b.Draw(GameMenu.SquareTexture, new Rectangle(0, 0, ScreenWidth, ScreenWidth), null, Color.Black);

            foreach (IDrawable element in this.drawables)
            {
                element.Draw(b);
            }

            b.Draw(Game1.mouseCursors,
                new Rectangle(Game1.oldMouseState.X - 146 / 2, Game1.oldMouseState.Y - 384 / 2, Game1.tileSize / 2, Game1.tileSize / 2),
                new Rectangle(146, 384, 9, 9), Color.White);
        }

        public abstract void Update();

        public abstract void Resize();

        public static int ScreenWidth => Game1.graphics.GraphicsDevice.Viewport.Width;

        public static int ScreenHeight => Game1.graphics.GraphicsDevice.Viewport.Height;

        protected void OnSwitchToNewMenu(IMenu newMenu)
        {
            this.SwitchToNewMenu?.Invoke(this, new SwitchMenuEventArgs(newMenu));
        }
    }
}
