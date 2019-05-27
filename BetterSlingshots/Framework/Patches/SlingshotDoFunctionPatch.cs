using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace BetterSlingshots.Framework.Patches
{
    /// <summary>Handles automatic reload and infinite ammo. Also fixes projectile velocity and automatic firing.</summary>
    [HarmonyPatch]
    internal class SlingshotDoFunctionPatch
    {
        private static SObject lastFiredAmmo;

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Slingshot).GetMethod(nameof(Slingshot.DoFunction));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static void Prefix(Slingshot __instance)
        {
            if(BetterSlingshotsMod.Instance.IsAutoFire)
                BetterSlingshotsMod.Instance.Helper.Reflection.GetField<bool>(__instance, "canPlaySound").SetValue(false);

            SlingshotDoFunctionPatch.lastFiredAmmo = __instance.attachments[0];
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static void Postfix(Slingshot __instance)
        {
            SlingshotDoFunctionPatch.FixLastFiredProjectileVelocity(__instance);

            if (BetterSlingshotsMod.Instance.Config.InfiniteAmmo && SlingshotDoFunctionPatch.lastFiredAmmo != null)
            {
                SlingshotDoFunctionPatch.lastFiredAmmo.Stack++;

                if (SlingshotDoFunctionPatch.lastFiredAmmo.Stack == 1)
                    __instance.attachments[0] = SlingshotDoFunctionPatch.lastFiredAmmo;
            }

            if (BetterSlingshotsMod.Instance.Config.AutoReload && SlingshotDoFunctionPatch.lastFiredAmmo != null && SlingshotDoFunctionPatch.lastFiredAmmo.Stack == 0)
            {
                SlingshotDoFunctionPatch.Reload(__instance);
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Method names are defined by Harmony.")]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int seenOpCodes = 0;
            bool justChangedLoad = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_4)
                {
                    seenOpCodes++;

                    if (seenOpCodes == 2)
                    {
                        instruction.opcode = OpCodes.Ldc_I4_0;
                        justChangedLoad = true;
                    }
                }
                else if (instruction.opcode == OpCodes.Ble && justChangedLoad)
                {
                    instruction.opcode = OpCodes.Blt;
                }
                else
                {
                    justChangedLoad = false;
                }

                yield return instruction;
            }
        }

        private static void Reload(Tool slingshot)
        {
            Item matchingItem = Game1.player.Items.FirstOrDefault(item => item != null && item.ParentSheetIndex == SlingshotDoFunctionPatch.lastFiredAmmo.ParentSheetIndex);

            if (matchingItem is SObject obj)
            {
                slingshot.attachments[0] = obj;
                Game1.player.Items[Game1.player.Items.IndexOf(matchingItem)] = null;
            }
        }

        private static void FixLastFiredProjectileVelocity(Slingshot slingshot)
        {
            if (Game1.currentLocation.projectiles.Count == 0)
                return;

            Projectile firedProjectile = Game1.currentLocation.projectiles[Game1.currentLocation.projectiles.Count - 1];
            Vector2 newVelocity = Utils.GetCorrectVelocity(new Vector2(slingshot.aimPos.X, slingshot.aimPos.Y), slingshot.getLastFarmerToUse(), !BetterSlingshotsMod.Instance.Config.DisableReverseAiming);

            BetterSlingshotsMod.Instance.Helper.Reflection.GetField<NetFloat>(firedProjectile, "xVelocity").GetValue().Value = newVelocity.X;
            BetterSlingshotsMod.Instance.Helper.Reflection.GetField<NetFloat>(firedProjectile, "yVelocity").GetValue().Value = newVelocity.Y;
        }
    }
}
