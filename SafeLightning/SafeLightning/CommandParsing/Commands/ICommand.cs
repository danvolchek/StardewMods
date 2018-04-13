namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    /// A console command.
    /// </summary>
    internal interface ICommand
    {
        string Description { get; }
        bool Dangerous { get; }

        bool Handles(string name);

        string Parse(string[] args);
    }
}