using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Linq;
using System.Reflection;
using WinterGrass.LegacySaving;

namespace WinterGrass
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/

        /// <summary>The mod instance.</summary>
        internal static ModEntry Instance { get; private set; }

        /// <summary>The mod configuration.</summary>
        internal ModConfig Config;

        /// <summary>The legacy save handler.</summary>
        private LegacySaveConverter legacySaveConverter;

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModEntry.Instance = this;
            this.Config = helper.ReadConfig<ModConfig>();

            this.legacySaveConverter = new LegacySaveConverter(this.Helper.DirectoryPath);

            HarmonyInstance harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            helper.Events.GameLoop.Saved += this.GameLoop_Saved;
            helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            helper.Events.Player.InventoryChanged += this.Player_InventoryChanged;
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameLoop_Saved(object sender, SavedEventArgs e)
        {
            this.legacySaveConverter.DeleteSaveFile();
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.IsWinter)
            {
                this.FixGrassColor();
            }
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                return;
            }

            this.legacySaveConverter.SetSaveFilePath();

            if (Game1.IsWinter)
            {
                this.legacySaveConverter.AddGrassFromLegacySaveFile();
                this.FixGrassColor();
            }
        }

        /// <summary>Raised after items are added or removed to a player's inventory.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            // After the user places down a grass starter, fix the color of the newly placed grass
            if (e.IsLocalPlayer && Game1.IsWinter && e.Removed.Any(item => item.ParentSheetIndex == 297))
            {
                this.FixGrassColor();
            }
        }

        /// <summary>Changes the color of every piece of grass to be snowy</summary>
        private void FixGrassColor()
        {
            if (!Context.IsMainPlayer)
            {
                return;
            }

            foreach (Grass grass in Game1.locations.Where(loc => loc != null).SelectMany(loc => loc.terrainFeatures.Pairs).Select(item => item.Value).OfType<Grass>())
            {
                grass.grassSourceOffset.Value = 80;
            }
        }
    }
}
