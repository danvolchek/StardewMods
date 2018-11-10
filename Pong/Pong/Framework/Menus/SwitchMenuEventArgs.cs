using Pong.Framework.Interfaces;
using System;

namespace Pong.Framework.Menus
{
    internal class SwitchMenuEventArgs : EventArgs
    {
        public IMenu NewMenu { get; }

        public SwitchMenuEventArgs(IMenu newMenu)
        {
            this.NewMenu = newMenu;
        }

    }
}