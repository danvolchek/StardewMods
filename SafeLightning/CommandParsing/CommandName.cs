namespace SafeLightning.CommandParsing
{
    /// <summary>
    ///     How a command can be invoked, either through its full name or its short name.
    /// </summary>
    internal class CommandName
    {
        public CommandName(string fullName, string shortName)
        {
            this.FullName = fullName;
            this.ShortName = shortName;
        }

        public string FullName { get; }
        public string ShortName { get; }
    }
}