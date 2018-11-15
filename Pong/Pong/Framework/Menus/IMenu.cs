using System;
using Pong.Framework.Common;
using StardewModdingAPI.Events;

namespace Pong.Framework.Menus
{
    internal interface IMenu: IDrawable, IUpdateable
    {
        void Resize();
        event EventHandler<SwitchMenuEventArgs> SwitchToNewMenu;

        bool ButtonPressed(EventArgsInput e);
        void MouseStateChanged(EventArgsMouseStateChanged e);

        void BeforeMenuSwitch();
    }
}
