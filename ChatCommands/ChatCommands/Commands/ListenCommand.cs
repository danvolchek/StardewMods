using StardewModdingAPI;

namespace ChatCommands.Commands
{
    /// <summary>
    /// A command to toggle listening to console output.
    /// </summary>
    internal class ListenCommand : BaseCommand
    {
        private readonly NotifyingTextWriter writer;

        public ListenCommand(IMonitor monitor, ChatCommandsConfig config, NotifyingTextWriter writer) : base(monitor)
        {
            this.writer = writer;

            if (config.ListenToConsoleOnStartup)
                this.Handle(null, null);
        }

        /// <summary>
        ///     Adds this command to SMAPI.
        /// </summary>
        public override void Register(ICommandHelper helper)
        {
            helper.Add("listen", "Toggles displaying console output in the in game chat box.", this.Handle);
        }

        /// <summary>
        ///     Handles the command.
        /// </summary>
        private void Handle(string name, string[] args)
        {
            this.writer.ToggleForceNotify();
            this.Monitor.Log(
                this.writer.IsForceNotifying()
                    ? "Listening to console output..."
                    : "Stopped listening to console output.", LogLevel.Info);
        }
    }
}