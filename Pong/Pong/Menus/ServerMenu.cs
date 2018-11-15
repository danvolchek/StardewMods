using System.Collections.Generic;
using Pong.Framework.Common;
using Pong.Framework.Menus;
using Pong.Framework.Menus.Elements;
using StardewModdingAPI;

namespace Pong.Menus
{
    class ServerMenu : Menu
    {
        public override void Update()
        {
            if(Context.IsWorldReady)
                this.OnSwitchToNewMenu(new PlayerMenu());
            this.InitDrawables();
        }

        public override void Resize()
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<IDrawable> GetDrawables()
        {
            yield return new StaticTextElement("Server Menu!", ScreenWidth / 2, ScreenHeight / 2 - 50, true, true);
        }
    }
}
