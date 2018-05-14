using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace ChatCommands.Util
{
    /// <summary>
    /// Contains various utility methods.
    /// </summary>
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

        /// <summary>
        /// Strips the SMAPI prefix off of the given input.
        /// </summary>
        internal static string StripSMAPIPrefix(string input)
        {
            if (input.Length == 0)
                return input;

            return input[0] != '[' ? input : string.Join("", input.Substring(input.IndexOf(']') + 1)).TrimStart();
        }

        /// <summary>
        /// Converts a <see cref="ConsoleColor"/> to a <see cref="Color"/>.
        /// </summary>
        internal static Color ConvertConsoleColorToColor(ConsoleColor color)
        {
            if (color == ConsoleColor.White || color == ConsoleColor.Black)
                return DefaultCommandColor;

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
                return DefaultCommandColor;
            }
        }

        /// <summary>
        /// Whether the given input should be ignored.
        /// </summary>
        internal static bool ShouldIgnore(string input)
        {
            return SuppressConsolePatterns.Any(p => p.IsMatch(input));
        }

        /// <summary>
        ///     Parses a string into an array of arguments.
        /// </summary>
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

        /// <summary>
        /// Garble up visible text so users without the mod can't read it at a glance.
        /// </summary>
        internal static string EncipherText(string text, long key)
        {
            Random r = new Random((int)key);
            StringBuilder curr = new StringBuilder();
            foreach (char c in text)
                curr.Append((char)(c + (char)r.Next(-32, 32)));
            return string.Concat(curr.ToString().Reverse());
        }

        /// <summary>
        /// Ungarble up visible text so users without the mod can't read it at a glance.
        /// </summary>
        internal static string DecipherText(string text, long key)
        {
            Random r = new Random((int)key);
            StringBuilder curr = new StringBuilder();
            foreach (char c in text.Reverse())
                curr.Append((char)(c - (char)r.Next(-32, 32)));
            return curr.ToString();
        }
    }
}
