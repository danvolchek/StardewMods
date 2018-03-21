using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;

namespace Pong
{
    public class ModEntry : Mod
    {
        private PongGame game;

        public override void Entry(IModHelper helper)
        {

            game = new PongGame(helper);

            InputEvents.ButtonPressed += this.ButtonPressed;
            GraphicsEvents.OnPostRenderEvent += this.OnPostRender;
            GraphicsEvents.Resize += this.Resize;
            ControlEvents.MouseChanged += this.MouseChanged;
        }

        private void ButtonPressed(object sender, EventArgsInput e)
        {
            e.SuppressButton();
            if (e.Button == SButton.Space)
                game.Start();
            else if (e.Button == SButton.Escape)
            {
                if (game.HasStarted())
                    game.Reset();
                else
                {
                    Game1.quit = true;
                    Game1.exitActiveMenu();
                }
            }
            else if (e.Button == SButton.P)
                game.TogglePaused();
        }

        private void OnPostRender(object sender, EventArgs e)
        {
            game.Update();
            game.Draw(Game1.spriteBatch);
        }

        private void Resize(object sender, EventArgs e)
        {
            game.Resize();
        }

        private void MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            game.MouseChanged(e.NewPosition);
        }
    }
}
