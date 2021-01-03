using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatCommands.Util
{
    /// <summary>Contains various utility methods.</summary>
    internal static class Utils
    {
        private static readonly Color DefaultCommandColor = new Color(104, 214, byte.MaxValue);

        /// <summary>Regex patterns which match console messages to suppress from the console and log.</summary>
        /// <remarks>Taken from https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/Program.cs#L89. </remarks>
        private static readonly Regex[] SuppressConsolePatterns =
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

        /// <summary>Strips the SMAPI prefix off of the given input.</summary>
        internal static string StripSMAPIPrefix(string input)
        {
            if (input.Length == 0)
                return input;

            return input[0] != '[' ? input : string.Join("", input.Substring(input.IndexOf(']') + 1)).TrimStart();
        }

        /// <summary>Converts a <see cref="ConsoleColor" /> to a <see cref="Color" />.</summary>
        internal static Color ConvertConsoleColorToColor(ConsoleColor color, IDictionary<string, string> overrides)
        {
            string colorName = Enum.GetName(typeof(ConsoleColor), color);

            if (overrides.TryGetValue(colorName, out string newName))
            {
                return ColorFromNameOrDefault(newName);
            }

            if (color == ConsoleColor.White || color == ConsoleColor.Black)
                return DefaultCommandColor;

            return ColorFromNameOrDefault(colorName);
        }

        internal static void Validate(IDictionary<string, string> overrides, IMonitor monitor)
        {
            bool anyInvalid = false;
            string[] validConsoleColors = Enum.GetNames(typeof(ConsoleColor));

            foreach (KeyValuePair<string, string> mapping in overrides)
            {
                if (!validConsoleColors.Any(v => v == mapping.Key))
                {
                    monitor.Log($"Color override key {mapping.Key} is invalid.", LogLevel.Warn);
                    anyInvalid = true;
                }

                if (!ColorFromName(mapping.Value, out Color _))
                {
                    monitor.Log($"Color override value {mapping.Value} is invalid.", LogLevel.Warn);
                    anyInvalid = true;
                }
            }

            if (anyInvalid)
            {
                monitor.Log("See the mod page for a list of valid colors.", LogLevel.Warn);
            }
        }

        private static bool ColorFromName(string name, out Color color)
        {
            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                PropertyInfo colorInfo = typeof(Color).GetProperty(name, BindingFlags.Static | BindingFlags.Public);
                // ReSharper disable once PossibleNullReferenceException
                color = (Color)colorInfo.GetValue(typeof(Color));
                return true;
            }
            catch
            {
                color = DefaultCommandColor;
                return false;
            }
        }

        private static Color ColorFromNameOrDefault(string name)
        {
            ColorFromName(name, out Color color);
            return color;
        }

        /// <summary>Whether the given input should be ignored.</summary>
        internal static bool ShouldIgnore(string input)
        {
            return SuppressConsolePatterns.Any(p => p.IsMatch(input));
        }

        /// <summary>Parses a string into an array of arguments.</summary>
        /// <remarks> The same as SMAPI's argument parsing, which I also wrote :)</remarks>
        internal static string[] ParseArgs(string input)
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

        internal static ChatSnippet CopyChatSnippet(ChatSnippet snippet)
        {
            return snippet.message == null
                ? new ChatSnippet(snippet.emojiIndex)
                : new ChatSnippet(snippet.message, LocalizedContentManager.CurrentLanguageCode);
        }

        /// <summary>Garble up visible text so users without the mod can't read it at a glance.</summary>
        internal static string EncipherText(string text, long key)
        {
            Random r = new Random((int)key);
            StringBuilder curr = new StringBuilder();
            foreach (char c in text)
                curr.Append((char)(c + (char)r.Next(-32, 32)));
            return string.Concat(curr.ToString().Reverse());
        }

        /// <summary>Ungarble up visible text so users without the mod can't read it at a glance.</summary>
        internal static string DecipherText(string text, long key)
        {
            Random r = new Random((int)key);
            StringBuilder curr = new StringBuilder();
            foreach (char c in text.Reverse())
                curr.Append((char)(c - (char)r.Next(-32, 32)));
            return curr.ToString();
        }

        /// <summary>Whether color info should be included based on a message.</summary>
        /// <remarks>Almost equivalent to the vanilla flow.</remarks>
        /// <param name="finalText">The message.</param>
        /// <returns>Whether color info should be included.</returns>
        internal static bool ShouldIncludeColorInfo(List<ChatSnippet> finalText)
        {
            if (!string.IsNullOrEmpty(finalText[0].message) && finalText[0].message[0] == '/')
            {
                if (finalText[0].message.Split(' ')[0].Length > 1)
                    return false;
            }

            return true;
        }
    }
}
