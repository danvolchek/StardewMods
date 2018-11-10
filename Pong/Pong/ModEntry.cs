using Pong.Game;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace Pong
{
    public class ModEntry : Mod
    {
        private PongGame game;

        public override void Entry(IModHelper helper)
        {
            this.game = new PongGame(helper);

            InputEvents.ButtonPressed += this.ButtonPressed;
            GraphicsEvents.OnPostRenderEvent += this.OnPostRender;
            GraphicsEvents.Resize += this.Resize;
        }

        private void ButtonPressed(object sender, EventArgsInput e)
        {
            e.SuppressButton();
            if (e.Button == SButton.Space)
            {
                this.game.Start();
            }
            else if (e.Button == SButton.Escape)
            {
                if (this.game.HasStarted())
                {
                    this.game.Reset();
                }
                else
                {
                    Game1.quit = true;
                    Game1.exitActiveMenu();
                }
            }
            else if (e.Button == SButton.P)
            {
                this.game.TogglePaused();
            }
        }

        private void OnPostRender(object sender, EventArgs e)
        {
            this.game.Update();
            this.game.Draw(Game1.spriteBatch);
        }

        private void Resize(object sender, EventArgs e)
        {
            this.game.Resize();
        }
    }
}