using Pong.Game;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Interfaces;
using Pong.Framework.Menus;

namespace Pong
{
    public class ModEntry : Mod
    {
        private IMenu currentMenu;

        public override void Entry(IModHelper helper)
        {
            PongGame.SquareTexture = helper.Content.Load<Texture2D>("assets/square.png");
            PongGame.CircleTexture = helper.Content.Load<Texture2D>("assets/circle.png");

            this.SwitchToNewMenu(new StartScreen());

            GraphicsEvents.OnPostRenderEvent += this.OnPostRender;
            GraphicsEvents.Resize += this.Resize;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            SoundManager.PlayKeyPressSound();
            this.currentMenu?.ButtonPressed(e);
        }

        private void SwitchToNewMenuEvent(object sender, SwitchMenuEventArgs e)
        {
            this.SwitchToNewMenu(e.NewMenu);
        }

        private void SwitchToNewMenu(IMenu newMenu)
        {
            if (newMenu == null)
            {
                Game1.quit = true;
                Game1.exitActiveMenu();
                return;
            }

            this.currentMenu = newMenu;
            this.currentMenu.SwitchToNewMenu += this.SwitchToNewMenuEvent;
        }

        private void OnPostRender(object sender, EventArgs e)
        {
            this.currentMenu?.Update();
            this.currentMenu?.Draw(Game1.spriteBatch);
        }

        private void Resize(object sender, EventArgs e)
        {
            this.currentMenu?.Resize();
        }
    }
}