using Pong.Framework.Common;
using System;

namespace Pong.Framework.Menus
{
    internal interface IMenu : IDrawable, IUpdateable, IInputable
    {
        void Resize();

        event EventHandler<SwitchMenuEventArgs> SwitchToNewMenu;
    }
}
