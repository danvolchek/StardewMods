using StardewModdingAPI;

namespace ChatCommands.Commands
{
    /// <summary>
    /// A dummy command to provide help documentation for the reply command.
    /// </summary>
    internal class ReplyCommand : BaseCommand
    {
        public ReplyCommand(IMonitor monitor) : base(monitor)
        {
        }

        /// <summary>
        ///     Adds this command to SMAPI.
        /// </summary>
        public override void Register(ICommandHelper helper)
        {
            helper.Add("r", "Replies to the last whisper you were sent. Only works from the chat box.\n"
                            + "Usage: /r <message>",
                (name, args) => this.Monitor.Log("This command only works from the chat box.", LogLevel.Error));
        }
    }
}