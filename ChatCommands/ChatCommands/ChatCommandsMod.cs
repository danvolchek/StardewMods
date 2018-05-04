using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatCommands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ChatCommandsMod : Mod, ICommandHandler
    {
        private readonly Color defaultCommandColor = new Color(104, 214, byte.MaxValue);

        /// <summary>Regex patterns which match console messages to suppress from the console and log.</summary>
        /// <remarks>Taken from https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/Program.cs#L89. </remarks>
        private readonly Regex[] suppressConsolePatterns =
        {
            new Regex(@"^TextBox\.Selected is now '(?:True|False)'\.$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant),
            new Regex(@"^(?:FRUIT )?TREE: IsClient:(?:True|False) randomOutput: \d+$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant),
            new Regex(@"^loadPreferences\(\); begin", RegexOptions.Compiled | RegexOptions.CultureInvariant),
            new Regex(@"^savePreferences\(\); async=", RegexOptions.Compiled | RegexOptions.CultureInvariant),
            new Regex(@"^Multiplayer auth success$", RegexOptions.Compiled | RegexOptions.CultureInvariant),
            new Regex(@"^DebugOutput: added CLOUD", RegexOptions.Compiled | RegexOptions.CultureInvariant)
        };

        private CommandValidator commandValidator;

        private NotifyingTextWriter consoleNotifier;

        private ChatCommandsConfig modConfig;

        /// <summary>
        ///     Parses a string into an array of arguments.
        /// </summary>
        /// <remarks> The same as SMAPI's argument parsing, which I also wrote :)</remarks>
        /// <param name="input">The string to parse.</param>
        public static string[] ParseArgs(string input)
        {
            bool inQuotes = false;
            IList<string> args = new List<string>();
            StringBuilder currentArg = new StringBuilder();
            foreach (char c in input)
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (!inQuotes && char.IsWhiteSpace(c))
                {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
                else
                {
                    currentArg.Append(c);
                }

            args.Add(currentArg.ToString());
            return args.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
        }

        public bool CanHandle(string input)
        {
            return input.Length > 1 && this.commandValidator.IsValidCommand(input.Substring(1));
        }

        public override void Entry(IModHelper helper)
        {
            this.commandValidator = new CommandValidator(helper.ConsoleCommands);
            this.consoleNotifier = new NotifyingTextWriter(Console.Out, this.OnLineWritten);

            Console.SetOut(this.consoleNotifier);
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;

            this.modConfig = helper.ReadConfig<ChatCommandsConfig>();
            
            // ReSharper disable once ObjectCreationAsStatement
            new ListenCommand(helper.ConsoleCommands, this.Monitor, this.modConfig, this.consoleNotifier);
        }

        public void Handle(string input)
        {
            input = input.Substring(1);
            string[] parts = ParseArgs(input);

            if (parts[0] == "halp")
                parts[0] = "help";

            this.consoleNotifier.Notify(true);
            this.Helper.ConsoleCommands.Trigger(parts[0], parts.Skip(1).ToArray());
            this.consoleNotifier.Notify(false);
        }

        private Color ConvertConsoleColorToColor(ConsoleColor color)
        {
            if (color == ConsoleColor.White || color == ConsoleColor.Black)
                return this.defaultCommandColor;

            try
            {
                string name = Enum.GetName(typeof(ConsoleColor), color);
                // ReSharper disable once AssignNullToNotNullAttribute
                PropertyInfo colorInfo = typeof(Color).GetProperty(name, BindingFlags.Static | BindingFlags.Public);
                // ReSharper disable once PossibleNullReferenceException
                return (Color)colorInfo.GetValue(typeof(Color));
            }
            catch
            {
                return this.defaultCommandColor;
            }
        }

        private void OnLineWritten(char[] buffer, int index, int count)
        {
            string toWrite = string.Join("", buffer.Skip(index).Take(count)).Trim();
            string noPrefix = StripSMAPIPrefix(toWrite).Trim();

            if (this.ShouldIgnore(noPrefix))
                return;
            if (this.consoleNotifier.IsNotifying())
                toWrite = noPrefix;

            if (!string.IsNullOrWhiteSpace(toWrite))
                Game1.chatBox?.addMessage(toWrite, this.ConvertConsoleColorToColor(Console.ForegroundColor));
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            if (Game1.chatBox != null && Game1.chatBox is CommandChatBox) return;
            if (Game1.chatBox != null)
                Game1.onScreenMenus.Remove(Game1.chatBox);
            Game1.chatBox = new CommandChatBox(this.Helper.Reflection, this, this.modConfig);
            Game1.onScreenMenus.Add(Game1.chatBox);
            this.Monitor.Log("Replaced Chatbox", LogLevel.Trace);
        }

        private bool ShouldIgnore(string input)
        {
            return this.suppressConsolePatterns.Any(p => p.IsMatch(input));
        }

        private static string StripSMAPIPrefix(string input)
        {
            if (input.Length == 0)
                return input;

            return input[0] != '[' ? input : string.Join("", input.Substring(input.IndexOf(']') + 1)).TrimStart();
        }
    }
}