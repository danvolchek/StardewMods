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

        public override bool ButtonPressed(EventArgsInput e)
        {
            bool result = base.ButtonPressed(e);

            switch (e.Button)
            {
                case SButton.Escape:
                    this.OnSwitchToNewMenu(null);
                    return true;
            }

            return result;
        }

        protected override IEnumerable<IDrawable> GetDrawables()
        {
            int centerHeight = SpriteText.getHeightOfString("Single Player Multi Player");

            yield return new StaticTextElement("Pong", ScreenWidth / 2, ScreenHeight / 2 - centerHeight * 5, true, true);
            yield return new StaticTextElement("By Cat", ScreenWidth / 2, ScreenHeight / 2 - centerHeight * 4, true, true);
            yield return new StaticTextElement("Single Player", ScreenWidth / 2 - ScreenWidth / 4, ScreenHeight / 2, true, false, () => this.OnSwitchToNewMenu(new GameMenu()));
            yield return new StaticTextElement("Multi Player", ScreenWidth / 2 + ScreenWidth / 4, ScreenHeight / 2, true, false, () => this.OnSwitchToNewMenu(new ServerMenu()));

            int escHeight = SpriteText.getHeightOfString("Press Esc to exit");
            yield return new StaticTextElement("Press Esc to exit", 15, ScreenHeight - escHeight - 15, false, false, () => this.OnSwitchToNewMenu(null));

            if(ModEntry.Instance.Helper.ModRegistry.IsLoaded("Platonymous.ArcadePong"))
            {
                int routineWidth = SpriteText.getWidthOfString("< Routine <");
                int routineHeight = SpriteText.getHeightOfString("< Routine <");
                yield return new StaticTextElement("< Routine <", ScreenWidth - routineWidth - 15, ScreenHeight - routineHeight - 15, false, true);
            }
        }

        public override void Update()
        {
        }

        public override void Resize()
        {
        }
    }
}
