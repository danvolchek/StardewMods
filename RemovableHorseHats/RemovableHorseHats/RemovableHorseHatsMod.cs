using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace RemovableHorseHats
{
    public class RemovableHorseHatsMod : Mod
    {
        private IEnumerable<string> keysToRemoveHat;
        internal static bool IsRemoveHatKeyDown { get; private set; }

        public override void Entry(IModHelper helper)
        {
            RemovableHorseHatsConfig config = helper.ReadConfig<RemovableHorseHatsConfig>();
            this.keysToRemoveHat = config.KeysToRemoveHat.ToLowerInvariant().Split(new char[]{' '}).Select(item => item.Trim()).Where(item => !string.IsNullOrEmpty(item));

            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            InputEvents.ButtonReleased += this.InputEvents_ButtonReleased;


            HarmonyInstance harmony = HarmonyInstance.Create("cat.removablehorsehats");

            harmony.Patch(GetSDVType("Characters.Horse").GetMethod("checkAction"),
                new HarmonyMethod(typeof(HorseCheckActionPatch).GetMethod("Prefix")), null);
        }

        private void InputEvents_ButtonReleased(object sender, EventArgsInput e)
        {
            string key = e.Button.ToString().ToLowerInvariant();
            if (this.keysToRemoveHat.Any(item => item == key))
                IsRemoveHatKeyDown = false;
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            string key = e.Button.ToString().ToLowerInvariant();
            if (this.keysToRemoveHat.Any(item => item == key))
                IsRemoveHatKeyDown = true;
        }

        //Big thanks to Routine for this workaround for mac users.
        //https://github.com/Platonymous/Stardew-Valley-Mods/blob/master/PyTK/PyUtils.cs#L117
        /// <summary>Gets the correct type of the object, handling different assembly names for mac/linux users.</summary>
        private static Type GetSDVType(string type)
        {
            const string prefix = "StardewValley.";
            Type defaultSDV = Type.GetType(prefix + type + ", Stardew Valley");

            return defaultSDV ?? Type.GetType(prefix + type + ", StardewValley");
        }
    }
}