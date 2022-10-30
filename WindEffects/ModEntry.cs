using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using WindEffects.Framework;

namespace WindEffects
{
    public class ModEntry : Mod
    {
        private WaveManager manager;
        internal static bool debug;

        private Random rand = new Random();
        public static ModConfig config;
        private IGenericModConfigMenuApi cfgMenu; // Generic Mod Config Menu

        public override void Entry(IModHelper helper)
        {
            SpriteBatchExtensions.Init(this.Helper);

            this.manager = new WaveManager(this.Helper, this.Monitor);

            config = this.Helper.ReadConfig<ModConfig>();

            this.Helper.Events.Display.Rendered += Display_Rendered;
            this.Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            this.Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            this.Helper.Events.Player.Warped += Player_Warped;
            this.Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            this.Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            this.Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            this.Helper.Events.GameLoop.GameLaunched += OnGameLaunched; // Generic Mod Config Menu
            this.Helper.ConsoleCommands.Add("we", "Wind Effects console commands.\nUsage:\n\twe debug: enable/disable debugging", this.ConsoleCommand_Triggered);
        }

        private void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (e.NewTime % 300 == 0)
                this.manager.ChangeWavePattern(Game1.isDebrisWeather);
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.manager.Clear();
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            this.manager.ChangeWavePattern(Game1.isDebrisWeather);

            if (!Game1.isDebrisWeather)
                this.manager.DisableAutoSpawning = this.rand.NextDouble() > Convert.ToDouble(config.WindyDayChance);

            if (config.WindyDayChance != 0)
	        this.Monitor.Log($"Wind Effects are {(this.manager.DisableAutoSpawning ? "disabled" : "enabled")} for today.", LogLevel.Info);
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            this.manager.Clear();
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || (!Game1.shouldTimePass() || (!Game1.game1.IsActive && Game1.options.pauseWhenOutOfFocus)) || Game1.currentLocation == null)
                return;

            this.manager.Update(Game1.currentLocation);
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!debug)
                return;

            Random rand = new Random();

            if (e.Button.IsUseToolButton())
            {
                double amp = rand.NextDouble() * 50 + 10;
                double period = rand.NextDouble() / 100 + 0.005;
                double rotation = rand.NextDouble() * Math.PI * 2;
                double speed = rand.NextDouble() * 5 + 6;
                this.manager.DebugAdd(new Wave(e.Cursor.AbsolutePixels.X, e.Cursor.AbsolutePixels.Y, amp, period, rotation, speed));
            }

            if (e.Button == SButton.O)
                manager.ChangeWavePattern(rand.Next(2) == 0);
        }

        private void Display_Rendered(object sender, RenderedEventArgs e)
        {
            if (debug)
                this.manager.DebugDraw(e.SpriteBatch);
        }

        private void ConsoleCommand_Triggered(string name, string[] args)
        {
            if (name != "we")
                return;

            if (args.Length == 0)
            {
                this.Monitor.Log($"Must provide a subcommand. See help {name} for more info.", LogLevel.Error);
                return;
            }

            if (args[0].ToLower() == "debug")
            {
                debug = !debug;
                this.Monitor.Log($"Wind Effects debugging is now {(debug ? "enabled" : "disabled")}.", LogLevel.Info);
                return;
            }

            this.Monitor.Log($"Unknown subcommand {args[0]}. See help {name} for more info.", LogLevel.Error);
        }

	// Generic Mod Config Menu
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
	    // Get Generic Mod Config Menu's API (if installed)
            cfgMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");  
	    if (cfgMenu is null)
		return;
            
	    // Register mod
            cfgMenu.Register(
                mod: ModManifest,
                reset: () => config = new ModConfig(),
                save: () => Helper.WriteConfig(config)
            );

            // Add config options
            cfgMenu.AddNumberOption(
		mod: ModManifest,
		name: () => Helper.Translation.Get("config.wind_prob_name"),
                tooltip: () => Helper.Translation.Get("config.wind_prob_tooltip"),
                getValue: () => config.WindyDayChance, 
                setValue: value => config.WindyDayChance = value, min: 0, max: 1, interval: 0.1f
	    );

            cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.wind_bushes_name"),
                tooltip: () => Helper.Translation.Get("config.wind_bushes_tooltip"),
                getValue: () => config.ShakeBushes,
                setValue: value => config.ShakeBushes = value
            );

            cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.wind_crops_name"),
                tooltip: () => Helper.Translation.Get("config.wind_crops_tooltip"),
                getValue: () => config.ShakeCrops,
                setValue: value => config.ShakeCrops = value
            );

            cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.wind_grass_name"),
                tooltip: () => Helper.Translation.Get("config.wind_grass_tooltip"),
                getValue: () => config.ShakeGrass,
                setValue: value => config.ShakeGrass = value
            );

            cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.wind_trees_name"),
                tooltip: () => Helper.Translation.Get("config.wind_trees_tooltip"),
                getValue: () => config.ShakeTrees,
                setValue: value => config.ShakeTrees = value
            );
	}
    }
}
