using StardewModdingAPI;

namespace ChatCommands.Commands
{
    /// <summary>Base class for commands.</summary>
    internal abstract class BaseCommand : ICommand
    {
        protected IMonitor Monitor;

        protected BaseCommand(IMonitor monitor)
        {
            this.Monitor = monitor;
        }

        public abstract void Register(ICommandHelper helper);
    }
}
