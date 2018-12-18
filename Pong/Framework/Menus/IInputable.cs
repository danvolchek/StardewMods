using StardewModdingAPI.Events;

namespace Pong.Framework.Menus
{
    internal interface IInputable
    {
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event arguments.</param>
        bool OnButtonPressed(ButtonPressedEventArgs e);

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event arguments.</param>
        void OnCursorMoved(CursorMovedEventArgs e);
    }
}
