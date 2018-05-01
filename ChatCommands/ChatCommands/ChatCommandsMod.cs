using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ChatCommands
{
    public class ChatCommandsMod : Mod, ICommandHandler
    {
        private NotifyingTextWriter interceptor;
        private CommandValidifier commandValidifier;

        private Color defaultCommandColor = new Color(104, 214, byte.MaxValue);

        public override void Entry(IModHelper helper)
        {
            this.commandValidifier = new CommandValidifier(helper.ConsoleCommands);
            this.interceptor = new NotifyingTextWriter(Console.Out, this.OnLineWritten);

            Console.SetOut(this.interceptor);
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            if (Game1.chatBox == null || !(Game1.chatBox is CommandChatBox))
            {
                if (Game1.chatBox != null)
                    Game1.onScreenMenus.Remove(Game1.chatBox);
                Game1.chatBox = new CommandChatBox(this.Helper.Reflection, this);
                Game1.onScreenMenus.Add(Game1.chatBox);
                this.Monitor.Log("Replaced Chatbox", LogLevel.Trace);
            }
        }

        public void Handle(string input)
        {
            input = input.Substring(1);
            string[] parts = ParseArgs(input);

            if (parts[0] == "halp")
                parts[0] = "help";

            this.interceptor.isNotifying = true;
            this.Helper.ConsoleCommands.Trigger(parts[0], parts.Skip(1).ToArray());
            this.interceptor.isNotifying = false;
        }

        public bool CanHandle(string input)
        {
            return input.Length > 1 && this.commandValidifier.IsValidCommand(input.Substring(1));
        }

        private void OnLineWritten(char[] buffer, int index, int count)
        {
            string toWrite = StripSMAPIPrefix(string.Join("", buffer.Skip(index).Take(count)).Trim()).Trim();
            if (!string.IsNullOrWhiteSpace(toWrite))
                Game1.chatBox?.addMessage(toWrite, ConvertConsoleColorToColor(Console.ForegroundColor));
        }

        private Color ConvertConsoleColorToColor(ConsoleColor color)
        {
            if (color == ConsoleColor.White || color == ConsoleColor.Black)
                return this.defaultCommandColor;

            try
            {
                string name = Enum.GetName(typeof(ConsoleColor), color);
                PropertyInfo colorInfo = typeof(Color).GetProperty(name, BindingFlags.Static | BindingFlags.Public);
                return (Color)colorInfo.GetValue(typeof(Color));
            }
            catch
            {
                return this.defaultCommandColor;
            }

        }

        private string StripSMAPIPrefix(string input)
        {
            if (input.Length == 0)
                return input;

            if (input[0] != '[')
                return input;
            return string.Join("", input.Substring(input.IndexOf(']') + 1)).TrimStart();
        }

        /// <summary>
        /// Parses a string into an array of arguments.
        /// </summary>
        /// <remarks> The same as SMAPI's argument parsing, which I also wrote :)</remarks>
        /// <param name="input">The string to parse.</param>
        public static string[] ParseArgs(string input)
        {
            bool inQuotes = false;
            IList<string> args = new List<string>();
            StringBuilder currentArg = new StringBuilder();
            foreach (char c in input)
            {
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
                    currentArg.Append(c);
            }
            args.Add(currentArg.ToString());
            return args.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
        }
    }
}