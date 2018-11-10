using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pong.Framework.Menus;
using StardewModdingAPI.Events;

namespace Pong.Framework.Interfaces
{
    internal interface IMenu: IDrawable, IUpdateable
    {
        void Resize();
        event EventHandler<SwitchMenuEventArgs> SwitchToNewMenu;

        void ButtonPressed(EventArgsInput inputArgs);
    }
}
