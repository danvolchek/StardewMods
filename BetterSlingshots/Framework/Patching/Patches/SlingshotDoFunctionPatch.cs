using Harmony;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using StardewValley.Tools;

namespace BetterSlingshots.Framework.Patching.Patches
{
    [HarmonyPatch]
    internal class SlingshotDoFunctionPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Slingshot).GetMethod(nameof(Slingshot.DoFunction));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static void Prefix(Slingshot __instance)
        {
            PatchManager.Instance.BeforeFiringHook(__instance);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static void Postfix(Slingshot __instance)
        {
            PatchManager.Instance.AfterFiringHook(__instance);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "Method names are defined by Harmony.")]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int seenOpCodes = 0;
            bool justChangedLoad = false;

            foreach(CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_4)
                {
                    seenOpCodes++;

                    if (seenOpCodes == 2)
                    {
                        instruction.opcode = OpCodes.Ldc_I4_0;
                        justChangedLoad = true;
                    }
                } else if (instruction.opcode == OpCodes.Ble && justChangedLoad)
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
    }
}
