using StardewModdingAPI;
using System.Reflection;

namespace ChatCommands
{
    /// <summary>Checks whether a command is a real SMAPI command.</summary>
    /// <remarks>Reflecting into SMAPI internals is discouraged, but we need to know if the command is real or not :).</remarks>
    class CommandValidifier
    {
        private MethodInfo commandHelperGet;
        private object CommandHelper;
        public CommandValidifier(ICommandHelper helper)
        {
            FieldInfo info = helper.GetType().GetField("CommandManager", BindingFlags.NonPublic | BindingFlags.Instance);

            this.CommandHelper = info.GetValue(helper);
            this.commandHelperGet = this.CommandHelper.GetType().GetMethod("Get", BindingFlags.Public | BindingFlags.Instance);
        }

        public bool IsValidCommand(string input)
        {
            string first = ChatCommandsMod.ParseArgs(input)[0];
            if (first == "halp")
                return true;
            else if (first == "help")
                return false;
            else
                return this.commandHelperGet.Invoke(this.CommandHelper, new object[] { first }) != null;
        }
    }
}
