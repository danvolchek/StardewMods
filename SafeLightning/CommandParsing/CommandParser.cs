using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SafeLightning.CommandParsing.Commands;
using StardewModdingAPI;

namespace SafeLightning.CommandParsing
{
    /// <summary>Registers and parses command line arguments.</summary>
    internal class CommandParser
    {
        /*********
        ** Fields
        *********/

        /// <summary>The monitor used for command output.</summary>
        private readonly IMonitor _monitor;

        /// <summary>The command helper used to register commands.</summary>
        private readonly ICommandHelper _commandHelper;

        /// <summary>All known commands</summary>
        private readonly IList<BaseCommand> _commands = new List<BaseCommand>();

        /// <summary>All known command descriptions</summary>
        private readonly IDictionary<string, string> _descriptions = new Dictionary<string, string>();

        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">The monitor used for command output.</param>
        /// <param name="commandHelper">The command helper used to register commands.</param>
        public CommandParser(IMonitor monitor, ICommandHelper commandHelper)
        {
            _monitor = monitor;
            _commandHelper = commandHelper;

            foreach (var commandType in Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(BaseCommand))))
            {
                _commands.Add(Activator.CreateInstance(commandType, _monitor) as BaseCommand);
            }
        }

        /// <summary>Registers all known commands.</summary>
        public void RegisterCommands()
        {
            //Register both a long and short command, hiding dangerous commands
            foreach (var s in new[] { "safe_lightning", "sl" })
            {
                var helpString = _commands.Where(command => !command.Dangerous).Aggregate("Safe lightning related debug commands.\n\nUsage:\n", (current, command) => current + $"   {s} {command.Description} \n");

                _descriptions.Add(s, helpString);
                _commandHelper.Add(s, helpString, ParseCommand);
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
                _monitor.Log(_descriptions[commandName], LogLevel.Info);
            }
            else if (!Context.IsWorldReady)
            {
                _monitor.Log("Commands require a save to be loaded.", LogLevel.Error);
            }
            else
            {
                var name = args[0].ToLowerInvariant();

                var toInvoke = _commands.FirstOrDefault(command => command.Dangerous ? name == command.FullName : name == command.FullName || name == command.ShortName);

                if (toInvoke != null)
                {
                    toInvoke.Invoke(args);
                }
                else
                {
                    PrintInvalidUsageError(commandName, $"Unrecognized command '{args[0]}'.");
                }
            }
        }

        /// <summary>Prints an invalid usage error.</summary>
        /// <param name="commandName"></param>
        /// <param name="message"></param>
        private void PrintInvalidUsageError(string commandName, string message)
        {
            _monitor.Log(message, LogLevel.Error);
            _monitor.Log($"Run help {commandName} for more info.", LogLevel.Error);
        }
    }
}
