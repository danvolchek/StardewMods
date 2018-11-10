using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Interfaces;
using Pong.Game;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pong.Framework.Menus
{
    internal abstract class Menu : IMenu
    {
        public event EventHandler<SwitchMenuEventArgs> SwitchToNewMenu;

        public abstract void ButtonPressed(EventArgsInput inputArgs);

        public virtual void Draw(SpriteBatch b)
        {
            b.Draw(PongGame.SquareTexture, new Rectangle(0, 0, ScreenWidth, ScreenWidth), null, Color.Black);
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
