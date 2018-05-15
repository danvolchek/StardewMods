using StardewModdingAPI;

namespace ChatCommands.Commands
{
    /// <summary>
    ///     Command interface.
    /// </summary>
    internal interface ICommand
    {
        void Register(ICommandHelper helper);
    }
}