using StardewModdingAPI;
using System.Linq;

namespace BetterSlingshots.Framework.Config
{
    internal class ConfigManager
    {
        private readonly IModHelper helper;
        private const string CONFIG_PATH = "config.json";

        public ConfigManager(IModHelper helper)
        {
            this.helper = helper;
        }

        public BetterSlingshotsModConfig GetConfig()
        {
            BetterSlingshotsModConfig config = this.ReadConfig();
            this.ValidateConfig(config);
            return config;
        }

        private void ValidateConfig(BetterSlingshotsModConfig config)
        {
            if (config.GalaxySlingshotPrice >= 0)
            {
                return;
            }

            config.GalaxySlingshotPrice = new BetterSlingshotsModConfig().GalaxySlingshotPrice;
            this.helper.WriteConfig(config);
        }

        private BetterSlingshotsModConfig ReadConfig()
        {
            try
            {
                // New config format - return it.
                return this.helper.ReadConfig<BetterSlingshotsModConfig>();
            }
            catch
            {
                BetterSlingshotsModConfig config;
                try
                {
                    // Old config format - copy from it.
                    LegacyConfig legacyConfig = this.helper.Data.ReadJsonFile<LegacyConfig>(ConfigManager.CONFIG_PATH);
                    config = new BetterSlingshotsModConfig(legacyConfig.DisableReverseAiming, legacyConfig.AutoReload, legacyConfig.AutomaticSlingshots.Split(',').Select(item => item.Trim()).ToArray(),
                        legacyConfig.ShowActualMousePositionWhenAiming, legacyConfig.CanMoveWhileFiring, legacyConfig.InfiniteAmmo, legacyConfig.RapidFire, legacyConfig.GalaxySlingshotPrice);
                }
                catch
                {
                    // Invalid in general - generate default config.
                    config = new BetterSlingshotsModConfig();
                }

                this.helper.WriteConfig(config);
                return config;
            }
        }
    }
}
