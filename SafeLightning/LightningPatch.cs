using Harmony;
using StardewValley;
using System.Reflection;

namespace SafeLightning
{
    [HarmonyPatch]
    class LightningPatch
    {
        private static MethodBase TargetMethod()
        {
            return typeof(Utility).GetMethod("performLightningUpdate");
        }

        private static bool Prefix()
        {
            SafeLightningMod.StrikeLightningSafely();

            return false;
        }
    }
}
