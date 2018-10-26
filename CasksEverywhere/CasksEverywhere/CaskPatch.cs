using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using StardewValley;
using StardewValley.Locations;

namespace CasksEverywhere
{
    [HarmonyPatch]
    class CaskPatch
    {
        private static MethodBase TargetMethod()
        {
            return CasksEverywhereMod.GetSDVType("Objects.Cask").GetMethod("performObjectDropInAction");
        }

        /// <summary>
        /// Change the first 0.4 in the method to the specified transparency in the config.
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool changed = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!changed && instruction.opcode == OpCodes.Isinst &&
                    (Type)instruction.operand == typeof(Cellar))
                {
                    changed = true;
                    instruction.operand = typeof(GameLocation);
                }

                yield return instruction;
            }
        }
    }
}
