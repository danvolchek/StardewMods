namespace ChatCommands
{
    /// <summary>Interface for command handling.</summary>
    internal interface ICommandHandler
    {
        void Handle(string input);

        bool CanHandle(string input);
    }
}
