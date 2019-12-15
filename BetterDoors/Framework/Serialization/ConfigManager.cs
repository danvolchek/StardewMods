using StardewModdingAPI;

namespace BetterDoors.Framework.Serialization
{
    /// <summary>Reads and handles config file validation.</summary>
    internal class ConfigManager
    {
        /*********
        ** Fields
        *********/

        /// <summary>Provides simplified APIs for writing mods.</summary>
        private readonly IModHelper helper;

        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public ConfigManager(IModHelper helper)
        {
            this.helper = helper;
        }

        /// <summary>Gets a user's validated config options.</summary>
        /// <returns>The config options.</returns>
        public BetterDoorsModConfig GetConfig()
        {
            BetterDoorsModConfig config = this.helper.ReadConfig<BetterDoorsModConfig>();

            if (ConfigManager.ValidateConfig(ref config))
                this.helper.WriteConfig(config);

            return config;
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Validate config options, updating invalid values.</summary>
        /// <param name="config">The config to validate.</param>
        /// <returns>Whether the config was invalid.</returns>
        private static bool ValidateConfig(ref BetterDoorsModConfig config)
        {
            if (config == null)
            {
                config = new BetterDoorsModConfig();
                return true;
            }

            // Only allow positive radii
            if (config.DoorToggleRadius < 1)
            {
                config.DoorToggleRadius = new BetterDoorsModConfig().DoorToggleRadius;
                return true;
            }

            return false;
        }
    }
}
