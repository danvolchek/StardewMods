using ChatCommands.Commands;
using StardewModdingAPI;

namespace ChatCommands
{
    internal class ListenCommand : ICommand
    {
        private readonly NotifyingTextWriter writer;
        private readonly IMonitor monitor;

        public ListenCommand(IMonitor monitor, ChatCommandsConfig config, NotifyingTextWriter writer)
        {
            this.writer = writer;
            this.monitor = monitor;

            if (config.ListenToConsoleOnStartup)
                this.Handle(null, null);
        }

        /// <summary>Adds this command to SMAPI.</summary>
        public void Register(ICommandHelper helper)
        {
            helper.Add("listen", "Toggles displaying console output in the in game chat box.", this.Handle);
        }

        /// <summary>Handles the command.</summary>
        private void Handle(string name, string[] args)
        {
            this.writer.ToggleForceNotify();
            this.monitor.Log(
                this.writer.IsForceNotifying()
                    ? "Listening to console output..."
                    : "Stopped listening to console output.", LogLevel.Info);
        }
    }
}
