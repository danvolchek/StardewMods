namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    /// Base class implementing common features commands will need.
    /// </summary>
    internal abstract class BaseCommand : ICommand
    {
        private string description;
        private CommandName name;
        private bool dangerous = false;

        public string Description { get { return Dangerous ? description : $"{name.FullName} ({name.ShortName})\n    - {description}"; } }
        public bool Dangerous { get { return dangerous; } }

        /// <summary>
        /// Creates a non dangerous <see cref="BaseCommand"/>.
        /// </summary>
        /// <param name="fullName">The long name of the command</param>
        /// <param name="shortName">The short name of the command</param>
        /// <param name="desc">The command description</param>
        public BaseCommand(string fullName, string shortName, string desc)
        {
            this.name = new CommandName(fullName.ToLower(), shortName.ToLower());
            this.description = desc;
        }

        /// <summary>
        /// Creates a dangerous <see cref="BaseCommand"/>.
        /// </summary>
        /// <param name="fullName">The long name of the command</param>
        /// <param name="desc">The command description</param>
        public BaseCommand(string fullName, string desc) : this(fullName, "", desc.ToUpper())
        {
            this.dangerous = true;
        }

        /// <summary>
        /// Whether this command can handle the given input. Dangerous commands must be called with their full name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Handles(string name)
        {
            return name.ToLower() == this.name.FullName || (!Dangerous && name.ToLower() == this.name.ShortName);
        }

        public abstract string Parse(string[] args);
    }
}