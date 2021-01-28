using Harmony;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomTransparency.Patches
{
    [HarmonyPatch]
    internal class FruitTreePatch
    {
        private static MethodBase TargetMethod()
        {
            return typeof(FruitTree).GetMethod(nameof(FruitTree.tickUpdate));
        }

        /// <summary>Change the first 0.4 in the method to the specified transparency in the config.</summary>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool changed = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!changed && instruction.opcode == OpCodes.Ldc_R4 &&
                    (float)instruction.operand <= 0.41f && (float)instruction.operand >= 0.39f)
                {
                    changed = true;
                    instruction.operand = ModEntry.Config.MinimumTreeTransparency;
                }

                yield return instruction;
            }
        }
    }
}