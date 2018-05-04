using StardewModdingAPI;

namespace ChatCommands
{
    internal class ListenCommand
    {
        private readonly NotifyingTextWriter writer;
        private readonly IMonitor monitor;

        public ListenCommand(ICommandHelper helper, IMonitor monitor, ChatCommandsConfig config, NotifyingTextWriter writer)
        {
            this.writer = writer;
            this.monitor = monitor;

            helper.Add("listen", "Toggles displaying console output in the in game chat box.", this.Handle);

            if (config.ListenToConsoleOnStartup)
                this.Handle(null, null);
        }

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