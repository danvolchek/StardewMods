using System.Collections.Generic;
using System.Linq;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Characters;

namespace RemovableHorseHats
{
    public class RemovableHorseHatsMod : Mod
    {
        private IEnumerable<string> keysToRemoveHat;
        internal static bool IsRemoveHatKeyDown { get; private set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            RemovableHorseHatsConfig config = helper.ReadConfig<RemovableHorseHatsConfig>();
            this.keysToRemoveHat = config.KeysToRemoveHat.ToLowerInvariant().Split(' ').Select(item => item.Trim()).Where(item => !string.IsNullOrEmpty(item));

            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;
            InputEvents.ButtonReleased += this.InputEvents_ButtonReleased;


            HarmonyInstance harmony = HarmonyInstance.Create("cat.removablehorsehats");

            harmony.Patch(typeof(Horse).GetMethod(nameof(Horse.checkAction)),
                new HarmonyMethod(typeof(HorseCheckActionPatch).GetMethod(nameof(HorseCheckActionPatch.Prefix))), null);
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
    }
}