using Pong.Framework.Menus;
using Pong.Framework.Menus.Elements;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.BellsAndWhistles;
using System.Collections.Generic;
using IDrawable = Pong.Framework.Common.IDrawable;

namespace Pong.Menus
{
    internal class StartMenu : Menu
    {
        public StartMenu()
        {
            this.InitDrawables();
        }

        public override bool ButtonPressed(EventArgsInput inputArgs)
        {
            switch (inputArgs.Button)
            {
                case SButton.Escape:
                    this.OnSwitchToNewMenu(null);
                    return true;
                case SButton.Space:
                    this.OnSwitchToNewMenu(new GameMenu());
                    return true;
            }

            return false;
        }

        protected override IEnumerable<IDrawable> GetDrawables()
        {
            int centerHeight = SpriteText.getHeightOfString("Single Player Multi Player");

            yield return new StaticTextElement("Pong", ScreenWidth / 2, ScreenHeight / 2 - centerHeight * 5);
            yield return new StaticTextElement("By Cat", ScreenWidth / 2, ScreenHeight / 2 - centerHeight * 4);
            yield return new StaticTextElement("Single Player", ScreenWidth / 2 - ScreenWidth / 4, ScreenHeight / 2);
            yield return new StaticTextElement("Multi Player", ScreenWidth / 2 + ScreenWidth / 4, ScreenHeight / 2);

            int escHeight = SpriteText.getHeightOfString("Press Esc to exit");
            yield return new StaticTextElement("Press Esc to exit", 0, ScreenHeight - escHeight);
        }

        public override void Update()
        {
        }

        public override void Resize()
        {
        }
    }
}
