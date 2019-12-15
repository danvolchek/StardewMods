using StardewValley;
using StardewValley.Locations;

namespace CustomizableDeathPenalty
{
    internal class PlayerStateManager
    {
        private static ModConfig config;
        public static PlayerState state;

        /***
        * Save the current values of fields that can change due to death.
        ***/

        public static void SaveState()
        {
            state = new PlayerState(Game1.player.Money, Game1.player.deepestMineLevel,
                Game1.mine == null ? -1 : MineShaft.lowestLevelReached, Game1.player.Items);
        }

        /***
         * Reset fields that could have change due to death according to the config file.
         ***/

        public static void LoadState()
        {
            if (config.KeepMoney)
            {
                Game1.player.Money = state.money;
            }

            if (config.RememberMineLevels)
            {
                Game1.player.deepestMineLevel = state.deepestMineLevel;
                if (state.lowestLevelReached != -1)
                    MineShaft.lowestLevelReached = state.lowestLevelReached;
            }

            if (config.RestoreStamina)
            {
                Game1.player.Stamina = Game1.player.MaxStamina;
            }

            if (config.RestoreHealth)
            {
                Game1.player.health = Game1.player.maxHealth;
            }

            if (config.KeepItems)
            {
                Game1.player.Items.Clear();
                Game1.player.items.AddRange(state.inventory);
            }

            state = null;
        }

        public static void SetConfig(ModConfig config)
        {
            PlayerStateManager.config = config;
        }
    }
}
