using System.Reflection;
using Harmony;

namespace NoCrows
{
    [HarmonyPatch]
    internal class AddCrowsPatch
    {
        private static MethodBase TargetMethod()
        {
            return NoCrowsMod.GetSDVType("Farm").GetMethod("addCrows");
        }

        private static bool Prefix()
        {
            return false;
        }
    }
}