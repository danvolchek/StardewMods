using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Menus;
using Pong.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace Pong
{
    public class ModEntry : Mod
    {
        private IMenu currentMenu;

        public override void Entry(IModHelper helper)
        {
            GameMenu.SquareTexture = helper.Content.Load<Texture2D>("assets/square.png");
            GameMenu.CircleTexture = helper.Content.Load<Texture2D>("assets/circle.png");

            this.SwitchToNewMenu(new StartMenu());

            GraphicsEvents.OnPostRenderEvent += this.OnPostRender;
            GraphicsEvents.Resize += this.Resize;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            ControlEvents.MouseChanged += this.ControlEvents_MouseChanged;
        }

        private void ControlEvents_MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            this.currentMenu?.MouseStateChanged(e);
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            e.SuppressButton();
            if(this.currentMenu?.ButtonPressed(e) == true)
                SoundManager.PlayKeyPressSound();
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