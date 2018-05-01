namespace ChatCommands
{
    internal interface ICommandHandler
    {
        void Handle(string input);

        bool CanHandle(string input);
    }
}