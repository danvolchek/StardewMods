using SafeLightning.CommandParsing.Commands;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SafeLightning.CommandParsing
{
    /// <summary>Registers and parses command line arguments.</summary>
    internal class CommandParser
    {
        /*********
        ** Fields
        *********/

        /// <summary>The monitor used for command output.</summary>
        private readonly IMonitor monitor;

        /// <summary>The command helper used to register commands.</summary>
        private readonly ICommandHelper commandHelper;

        /// <summary>All known commands</summary>
        private readonly IList<BaseCommand> commands = new List<BaseCommand>();

        /// <summary>All known command descriptions</summary>
        private readonly IDictionary<string, string> descriptions = new Dictionary<string, string>();

        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">The monitor used for command output.</param>
        /// <param name="commandHelper">The command helper used to register commands.</param>
        public CommandParser(IMonitor monitor, ICommandHelper commandHelper)
        {
            this.monitor = monitor;
            this.commandHelper = commandHelper;

            foreach (Type commandType in Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(BaseCommand))))
            {
                this.commands.Add(Activator.CreateInstance(commandType, this.monitor) as BaseCommand);
            }
        }

        /// <summary>Registers all known commands.</summary>
        public void RegisterCommands()
        {
            //Register both a long and short command, hiding dangerous commands
            foreach (string s in new[] { "safe_lightning", "sl" })
            {
                string helpString = this.commands.Where(command => !command.Dangerous).Aggregate("Safe lightning related debug commands.\n\nUsage:\n", (current, command) => current + $"   {s} {command.Description} \n");

                this.descriptions.Add(s, helpString);
                this.commandHelper.Add(s, helpString, this.ParseCommand);
            }
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Parses the given command.</summary>
        /// <param name="commandName">The name used to call the command.</param>
        /// <param name="args">The command arguments.</param>
        private void ParseCommand(string commandName, string[] args)
        {
            if (args.Length == 0)
            {
                this.monitor.Log(this.descriptions[commandName], LogLevel.Info);
            }
            else if (!Context.IsWorldReady)
            {
                this.monitor.Log("Commands require a save to be loaded.", LogLevel.Error);
            }
            else
            {
                string name = args[0].ToLowerInvariant();

                BaseCommand toInvoke = this.commands.FirstOrDefault(command => command.Dangerous ? name == command.FullName : name == command.FullName || name == command.ShortName);

                if (toInvoke != null)
                {
                    toInvoke.Invoke(args);
                }
                else
                {
                    this.PrintInvalidUsageError(commandName, $"Unrecognized command '{args[0]}'.");
                }
            }
        }

        /// <summary>Prints an invalid usage error.</summary>
        /// <param name="commandName"></param>
        /// <param name="message"></param>
        private void PrintInvalidUsageError(string commandName, string message)
        {
            this.monitor.Log(message, LogLevel.Error);
            this.monitor.Log($"Run help {commandName} for more info.", LogLevel.Error);
        }
    }
}
