using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Menus;
using Pong.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using Pong.Framework.Common;

namespace Pong
{
    public class ModEntry : Mod
    {
        //TODO: Highlightable drawables, then multiplayer connection menu (both join and host)
        //TODO: then player menu, then remoteBall, maybe remoteGame?
        private IMenu currentMenu;

        internal static ModEntry Instance;

        public override void Entry(IModHelper helper)
        {
            Instance = this;

            AssetManager.Init(helper);
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
            if (this.currentMenu == null)
                return;

            e.SuppressButton();
            if(this.currentMenu.ButtonPressed(e))
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