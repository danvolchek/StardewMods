using StardewModdingAPI;

namespace ChatCommands.Commands
{
    /// <summary>
    ///     A dummy command to provide help documentation for the whisper command.
    /// </summary>
    internal class WhisperCommand : BaseCommand
    {
        public WhisperCommand(IMonitor monitor) : base(monitor)
        {
        }

        /// <summary>
        ///     Adds this command to SMAPI.
        /// </summary>
        public override void Register(ICommandHelper helper)
        {
            helper.Add("w", "Send a message to only one player. Only works from the chat box.\n"
                            + "Usage: /w <name> <message>",
                (name, args) => this.Monitor.Log("This command only works from the chat box.", LogLevel.Error));
        }
    }
}