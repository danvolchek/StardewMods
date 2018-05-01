using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ChatCommands
{
    public class ChatCommandsMod : Mod, ICommandHandler
    {
        private NotifyingTextWriter interceptor;
        private CommandValidifier commandValidifier;

        public override void Entry(IModHelper helper)
        {
            this.interceptor = new NotifyingTextWriter(Console.Out, this.OnLineWritten);
            this.commandValidifier = new CommandValidifier(helper.ConsoleCommands);

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

            this.interceptor.isRedirecting = true;
            this.Helper.ConsoleCommands.Trigger(parts[0], parts.Skip(1).ToArray());
            this.interceptor.isRedirecting = false;
        }

        public bool CanHandle(string input)
        {
            return this.commandValidifier.IsValidCommand(input.Substring(1));
        }

        private void OnLineWritten(char[] buffer, int index, int count)
        {
            string toWrite = StripSMAPIPrefix(string.Join("", buffer.Skip(index).Take(count)).Trim()).Trim();
            if (!string.IsNullOrWhiteSpace(toWrite))
                Game1.chatBox?.addMessage(toWrite, new Color(104, 214, byte.MaxValue));
        }

        private string StripSMAPIPrefix(string input)
        {
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
            IList<char> currentArg = new List<char>();
            foreach (char c in input)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (!inQuotes && char.IsWhiteSpace(c))
                {
                    args.Add(string.Concat(currentArg));
                    currentArg.Clear();
                }
                else
                    currentArg.Add(c);
            }
            args.Add(string.Concat(currentArg));
            return args.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
        }
    }
}
