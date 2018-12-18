using System.Reflection;
using Harmony;
using StardewModdingAPI;

namespace CustomTransparency
{
    public class CustomTransparencyMod : Mod
    {
        internal static CustomTransparencyModConfig Config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            HarmonyInstance instance = HarmonyInstance.Create("cat.customtransparency");

            if (ValidateConfig(helper.ReadConfig<CustomTransparencyModConfig>(), out Config))
                helper.WriteConfig(Config);

            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Validate the given config, resulting in a new config with values changed to the default if invalid.
        /// </summary>
        private static bool ValidateConfig(CustomTransparencyModConfig config, out CustomTransparencyModConfig validatedConfig)
        {
            bool changed = false;

            validatedConfig = new CustomTransparencyModConfig();

            if (config.MinimumBuildingTransparency >= 0 && config.MinimumBuildingTransparency <= 1)
                validatedConfig.MinimumBuildingTransparency = config.MinimumBuildingTransparency;
            else changed = true;

            if (config.MinimumTreeTransparency >= 0 && config.MinimumTreeTransparency <= 1)
                validatedConfig.MinimumTreeTransparency = config.MinimumTreeTransparency;
            else changed = true;

            return changed;
        }
    }
}