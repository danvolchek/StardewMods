using StardewModdingAPI;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>Common base class for commands.</summary>
    internal abstract class BaseCommand
    {
        /*********
        ** Fields
        *********/

        /// <summary>The command description.</summary>
        private readonly string description;

        /// <summary>The monitor used for command output.</summary>
        protected readonly IMonitor monitor;

        /*********
        ** Accessors
        *********/

        /// <summary>The full name of the command.</summary>
        public string FullName { get; }

        /// <summary>The short name of the command.</summary>
        public string ShortName { get; }

        /// <summary>The command description.</summary>
        public string Description => this.Dangerous ? this.description : $"{this.FullName} ({this.ShortName})\n    - {this.description}";

        /// <summary>Whether the command is dangerous or not.</summary>
        public bool Dangerous { get; }

        /*********
         ** Protected methods
         *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">The monitor used for command output.</param>
        /// <param name="fullName">The full name of the command.</param>
        /// <param name="shortName">The short name of the command.</param>
        /// <param name="desc">The command description.</param>
        protected BaseCommand(IMonitor monitor, string fullName, string shortName, string desc)
        {
            this.monitor = monitor;
            this.ShortName = shortName.ToLowerInvariant();
            this.FullName = fullName.ToLowerInvariant();
            this.description = desc;
            this.Dangerous = shortName == "";
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">The monitor used for command output.</param>
        /// <param name="fullName">The full name of the command.</param>
        /// <param name="desc">The command description.</param>
        protected BaseCommand(IMonitor monitor, string fullName, string desc) : this(monitor, fullName, "", desc)
        {
        }

        /*********
         ** Public methods
         *********/

        /// <summary>Invokes the command.</summary>
        /// <param name="args">The command arguments</param>
        public abstract void Invoke(string[] args);
    }
}
