using StardewModdingAPI.Events;

namespace Pong.Framework.Menus
{
    internal interface IInputable
    {
        bool ButtonPressed(EventArgsInput e);
        void MouseStateChanged(EventArgsMouseStateChanged e);
    }
}
