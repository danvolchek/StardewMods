using StardewModdingAPI;
using System.Reflection;
using ChatCommands.Util;

namespace ChatCommands
{
    /// <summary>Checks whether a command is a real SMAPI command.</summary>
    /// <remarks>Reflecting into SMAPI internals is discouraged, but we need to know if the command is real or not :).</remarks>
    internal class CommandValidator
    {
        private readonly object commandHelper;
        private readonly MethodInfo commandHelperGet;

        public CommandValidator(ICommandHelper helper)
        {
            FieldInfo info = helper.GetType().GetField("CommandManager", BindingFlags.NonPublic | BindingFlags.Instance);


            this.commandHelper = info?.GetValue(helper);
            this.commandHelperGet = this.commandHelper?.GetType().GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
        }

        public bool IsValidCommand(string input)
        {
            string first = Utils.ParseArgs(input)[0];
            switch (first)
            {
                case "halp":
                    return true;
                case "help":
                    return false;
                default:
                    return this.commandHelperGet?.Invoke(this.commandHelper, new object[] { first }) != null;
            }
        }
    }
}