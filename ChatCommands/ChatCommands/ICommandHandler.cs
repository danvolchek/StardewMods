namespace ChatCommands
{
    interface ICommandHandler
    {
        void Handle(string input);
        bool CanHandle(string input);
    }
}
