using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;

namespace BetterSlingshots.Framework.Patches
{
    /// <summary>Faces the farmer the right way if reverse aiming is disabled.</summary>
    [HarmonyPatch]
    internal class SlingshotTickUpdatePatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prepare()
        {
            return BetterSlingshotsMod.Instance.Config.DisableReverseAiming;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Slingshot).GetMethod(nameof(Slingshot.tickUpdate));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static void Postfix(Slingshot __instance)
        {
            Farmer lastFarmer = __instance.getLastFarmerToUse();
            if (lastFarmer == null || !lastFarmer.usingSlingshot)
                return;

            if (lastFarmer.IsLocalPlayer)
                lastFarmer.faceGeneralDirection(new Vector2(__instance.aimPos.X, __instance.aimPos.Y));

            int num = lastFarmer.FacingDirection == 3 || lastFarmer.FacingDirection == 1 ? 1 : (lastFarmer.FacingDirection == 0 ? 2 : 0);
            lastFarmer.FarmerSprite.setCurrentFrame(42 + num);
        }
    }
}
