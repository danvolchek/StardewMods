using System;
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
        private HashSet<SButton> keysToRemoveHat;
        internal static bool IsRemoveHatKeyDown { get; private set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            RemovableHorseHatsConfig config = helper.ReadConfig<RemovableHorseHatsConfig>();
            this.keysToRemoveHat = new HashSet<SButton>(
                config.KeysToRemoveHat
                    .Split(' ')
                    .Select(raw => Enum.TryParse(raw, true, out SButton button) ? button : SButton.None)
                    .Where(key => key != SButton.None)
            );

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;


            HarmonyInstance harmony = HarmonyInstance.Create("cat.removablehorsehats");

            harmony.Patch(typeof(Horse).GetMethod(nameof(Horse.checkAction)),
                new HarmonyMethod(typeof(HorseCheckActionPatch).GetMethod(nameof(HorseCheckActionPatch.Prefix))), null);
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (this.keysToRemoveHat.Contains(e.Button))
                IsRemoveHatKeyDown = false;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this.keysToRemoveHat.Contains(e.Button))
                IsRemoveHatKeyDown = true;
        }
    }
}