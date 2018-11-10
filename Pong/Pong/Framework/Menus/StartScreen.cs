using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Game;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace Pong.Framework.Menus
{
    internal class StartScreen : Menu
    {
        public override void ButtonPressed(EventArgsInput inputArgs)
        {
            switch (inputArgs.Button)
            {
                case SButton.Escape:
                    this.OnSwitchToNewMenu(null);
                    break;
                case SButton.Space:
                    this.OnSwitchToNewMenu(new PongGame());
                    break;
            }
        }

        public override void Draw(SpriteBatch b)
        {
            base.Draw(b);
            int centerHeight = SpriteText.getHeightOfString("Press Space to start");
            SpriteText.drawStringHorizontallyCenteredAt(b, "Pong", ScreenWidth / 2,
                ScreenHeight / 2 - centerHeight * 5, 999999, -1, 999999, 1f, 0.88f, false,
                SpriteText.color_White);
            SpriteText.drawStringHorizontallyCenteredAt(b, "By Cat", ScreenWidth / 2,
                ScreenHeight / 2 - centerHeight * 4, 999999, -1, 999999, 1f, 0.88f, false,
                SpriteText.color_White);

            SpriteText.drawStringHorizontallyCenteredAt(b, "Press Space to start", ScreenWidth / 2,
                ScreenHeight / 2, 999999, -1, 999999, 1f, 0.88f, false, SpriteText.color_White);
            int escHeight = SpriteText.getHeightOfString("Press Esc to exit");
            SpriteText.drawString(b, "Press Esc to exit", 0, ScreenHeight - escHeight, 999999, -1, 999999, 1f,
                0.88f, false, -1, "", SpriteText.color_White);

            b.Draw(Game1.mouseCursors,
                new Rectangle(Game1.oldMouseState.X, Game1.oldMouseState.Y, Game1.tileSize / 2, Game1.tileSize / 2),
                new Rectangle(146, 384, 9, 9), Color.White);
        }

        public override void Update()
        {
        }

        public override void Resize()
        {
        }
    }
}
