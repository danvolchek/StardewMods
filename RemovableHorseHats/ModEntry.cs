using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RemovableHorseHats
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/

        /// <summary>The mod instance.</summary>
        internal static ModEntry Instance { get; private set; }

        /// <summary>The keys that let you remove the hat from the horse.</summary>
        private HashSet<SButton> keysToRemoveHat;

        /// <summary>Whether any of the keys to remove the hat are down.</summary>
        internal bool IsRemoveHatKeyDown { get; private set; }

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModConfig config = helper.ReadConfig<ModConfig>();
            this.keysToRemoveHat = new HashSet<SButton>(
                config.KeysToRemoveHat
                    .Split(' ')
                    .Select(raw => Enum.TryParse(raw, true, out SButton button) ? button : SButton.None)
                    .Where(key => key != SButton.None)
            );

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;

            ModEntry.Instance = this;
            HarmonyInstance harmony = HarmonyInstance.Create(this.Helper.ModRegistry.ModID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (this.keysToRemoveHat.Contains(e.Button))
            {
                this.IsRemoveHatKeyDown = false;
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this.keysToRemoveHat.Contains(e.Button))
            {
                this.IsRemoveHatKeyDown = true;
            }
        }
    }
}
