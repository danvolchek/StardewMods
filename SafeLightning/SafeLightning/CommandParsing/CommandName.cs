namespace SafeLightning.CommandParsing
{
    /// <summary>
    /// How a command can be invoked, either through its full name or its short name.
    /// </summary>
    internal class CommandName
    {
        public string FullName { get; }
        public string ShortName { get; }

        public CommandName(string fullName, string shortName)
        {
            FullName = fullName;
            ShortName = shortName;
        }
    }
}