using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Common;
using Pong.Framework.Menus;
using Pong.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Pong
{
    public class ModEntry : Mod
    {
        //TODO: then multiplayer connection menu (both join and host)
        //TODO: then player menu, then remoteBall, maybe remoteGame?
        private IMenu currentMenu;

        internal string PongId { get; private set; }

        internal static ModEntry Instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.PongId = this.ModManifest.UniqueID;
            Instance = this;

            if (!AssetManager.Init(helper))
            {
                this.Monitor.Log("Failed to load textures, exiting.", LogLevel.Error);
                return;
            }
            this.SwitchToNewMenu(new StartMenu());

            helper.Events.Display.Rendered += this.OnRendered;
            helper.Events.Display.WindowResized += this.OnWindowResized;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.CursorMoved += this.OnCursorMoved;
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            this.currentMenu?.OnCursorMoved(e);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this.currentMenu == null)
                return;

            //e.SuppressButton();
            if (this.currentMenu.OnButtonPressed(e))
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

        /// <summary>Raised after the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRendered(object sender, RenderedEventArgs e)
        {
            //if(this.i++ % 60 == 0)
            //{
                this.currentMenu?.Update();
            //    this.i = 0;
            //}

            this.currentMenu?.Draw(Game1.spriteBatch);

            /*for (int i = Menu.ScreenHeight / 2 - 25; i < Menu.ScreenHeight; i += 75)
            {
                Game1.spriteBatch.Draw(AssetManager.SquareTexture, new Rectangle(Menu.ScreenWidth / 2 - 25, i, 50, 50), Color.White);
                Game1.spriteBatch.Draw(AssetManager.SquareTexture, new Rectangle(Menu.ScreenWidth / 2 - 25, (Menu.ScreenHeight/2 - 25)*2 - i, 50, 50), Color.White);
            }
            for (int i = Menu.ScreenWidth / 2 - 25; i < Menu.ScreenWidth; i += 75)
            {
                Game1.spriteBatch.Draw(AssetManager.SquareTexture, new Rectangle(i, Menu.ScreenHeight / 2 - 25, 50, 50), Color.Red);
                Game1.spriteBatch.Draw(AssetManager.SquareTexture, new Rectangle((Menu.ScreenWidth / 2 - 25) * 2 - i, Menu.ScreenHeight / 2 - 25, 50, 50), Color.Red);
            }*/
        }

        /// <summary>Raised after the game window is resized.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            this.currentMenu?.Resize();
        }
    }
}