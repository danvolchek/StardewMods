using StardewModdingAPI;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ChatCommands
{
    /// <summary>Checks whether a command is a real SMAPI command.</summary>
    /// <remarks>Reflecting into SMAPI internals is discouraged, but we need to know if the command is real or not :).</remarks>
    internal class CommandValidator
    {
        private MethodInfo commandHelperGet;
        private object commandHelper;

        public CommandValidator(ICommandHelper helper)
        {
            FieldInfo info = helper.GetType().GetField("CommandManager", BindingFlags.NonPublic | BindingFlags.Instance);

            this.commandHelper = info.GetValue(helper);
            this.commandHelperGet = this.commandHelper.GetType().GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
        }

        public bool IsValidCommand(string input)
        {
            string first = ChatCommandsMod.ParseArgs(input)[0];
            if (first == "halp")
                return true;
            else if (first == "help")
                return false;
            else
                return this.commandHelperGet.Invoke(this.commandHelper, new object[] { first }) != null;
        }
    }
}