using SafeLightning.CommandParsing.Commands;
using StardewModdingAPI;
using System.Collections.Generic;

namespace SafeLightning.CommandParsing
{
    /// <summary>
    /// Registers and parses command line arguments.
    /// </summary>
    internal class CommandParser
    {
        private IList<ICommand> commands;
        private IMonitor monitor;

        private IDictionary<string, string> descriptions = new Dictionary<string, string>();

        public CommandParser(SafeLightningMod mod)
        {
            this.monitor = mod.Monitor;

            //Get available commands
            commands = new List<ICommand>
            {
                new GetLightningCommand(),
                new PrintLocationCommand(),
                new SetLightningCommand(),
                new GrowTreesCommand(),
                new RemoveFeaturesCommand()
            };

            //Register both a long and short command, hiding dangerous commands
            foreach (string s in new List<string>() { "safe_lightning", "sl" })
            {
                string helpString = "Safe lightning related debug commands.\n\nUsage:\n";
                foreach (ICommand command in commands)
                {
                    if (command.Dangerous)
                        continue;
                    helpString += $"   {s} {command.Description} \n";
                }
                descriptions.Add(s, helpString);
                mod.Helper.ConsoleCommands.Add(s, helpString, this.ParseCommand);
            }
        }

        /// <summary>
        /// Parses the given command.
        /// </summary>
        /// <param name="commandName">Name used to call this method</param>
        /// <param name="args">Command arguments</param>
        private void ParseCommand(string commandName, string[] args)
        {
            if (args.Length == 0)
            {
                monitor.Log(descriptions[commandName], LogLevel.Info);
            }
            else if (!Context.IsWorldReady)
            {
                monitor.Log("Commands require a save to be loaded.", LogLevel.Error);
            }
            else
            {
                foreach (ICommand command in commands)
                {
                    if (command.Handles(args[0]))
                    {
                        //If the command is dangerous, only show how to use it if user guessed name exactly right.
                        if (command.Dangerous)
                        {
                            if (args.Length > 2 || args.Length == 1 || (args.Length == 2 && args[1] != "--force"))
                                PrintInvalidUsageError(commandName, $"Unrecognized command '{args[0]}'.");
                            else
                                monitor.Log(command.Parse(args), LogLevel.Info);
                            return;
                        }
                        else
                        {
                            if (args.Length != 1)
                                PrintInvalidUsageError(commandName, $"Too many arguments for command '{args[0]}'.");
                            else
                                monitor.Log(command.Parse(args), LogLevel.Info);
                            return;
                        }
                    }
                }
                PrintInvalidUsageError(commandName, $"Unrecognized command '{args[0]}'.");
            }
        }

        private void PrintInvalidUsageError(string commandName, string type, bool append = true)
        {
            string toAppend = $"See help {commandName} for more info.";
            string[] split = type.Split('\n');
            if (split.Length == 1)
                monitor.Log($"{type} {(append ? toAppend : "")}", LogLevel.Error);
            else
            {
                foreach (string s in split)
                    monitor.Log($"{s}", LogLevel.Error);
                if (append)
                    monitor.Log(toAppend, LogLevel.Error);
            }
        }
    }
}