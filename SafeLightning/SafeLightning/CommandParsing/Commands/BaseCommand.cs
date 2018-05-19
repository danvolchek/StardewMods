namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    ///     Base class implementing common features commands will need.
    /// </summary>
    internal abstract class BaseCommand : ICommand
    {
        private readonly string description;
        private readonly CommandName name;

        /// <summary>
        ///     Creates a non dangerous <see cref="BaseCommand" />.
        /// </summary>
        /// <param name="fullName">The long name of the command</param>
        /// <param name="shortName">The short name of the command</param>
        /// <param name="desc">The command description</param>
        protected BaseCommand(string fullName, string shortName, string desc)
        {
            this.name = new CommandName(fullName.ToLower(), shortName.ToLower());
            this.description = desc;
        }

        /// <summary>
        ///     Creates a dangerous <see cref="BaseCommand" />.
        /// </summary>
        /// <param name="fullName">The long name of the command</param>
        /// <param name="desc">The command description</param>
        protected BaseCommand(string fullName, string desc) : this(fullName, "", desc.ToUpper())
        {
            this.Dangerous = true;
        }

        public string Description => this.Dangerous
            ? this.description
            : $"{this.name.FullName} ({this.name.ShortName})\n    - {this.description}";

        public bool Dangerous { get; }

        /// <summary>
        ///     Whether this command can handle the given input. Dangerous commands must be called with their full name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Handles(string name)
        {
            return name.ToLower() == this.name.FullName || !this.Dangerous && name.ToLower() == this.name.ShortName;
        }

        public abstract string Parse(string[] args);
    }
}