using System.Collections.Generic;
using System.Reflection;
using BetterDoors.Framework;
using BetterDoors.Framework.ContentPacks;
using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Mapping;
using BetterDoors.Framework.Serialization;
using BetterDoors.Framework.Utility;
using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BetterDoors
{
    //TODO:
    // - Double doors
    // - There's one more axis the doors could theoretically be opened on - decide whether it's feasible/worth it to add.
    // - Multiplayer

    /// <summary>
    /// A mod which provides better doors to map makers.
    /// </summary>
    public class BetterDoorsMod : Mod
    {
        private IList<LoadedContentPackDoorEntry> loadedContentPacks;
        private DoorPositionSerializer serializer;
        private CallbackTimer timer;

        private GeneratedSpriteManager spriteManager;
        private DoorSpriteGenerator generator;
        private DoorAssetLoader assetLoader;
        private MapModifier mapModifier;
        private DoorCreator creator;
        private DoorManager manager;

        internal static BetterDoorsMod Instance { get; private set; }

        public override void Entry(IModHelper helper)
        {
            this.loadedContentPacks = new ContentPackLoader(this.Helper, this.Monitor).LoadContentPacks();
            this.serializer = new DoorPositionSerializer(this.Helper.Data);
            this.timer = new CallbackTimer();

            this.spriteManager = new GeneratedSpriteManager();
            this.generator = new DoorSpriteGenerator(this.spriteManager, this.Monitor, Game1.graphics.GraphicsDevice);
            this.creator = new DoorCreator(this.spriteManager, this.timer, this.Monitor);
            this.assetLoader = new DoorAssetLoader(this.Helper.Content);
            this.mapModifier = new MapModifier();
            this.manager = new DoorManager();

            BetterDoorsMod.Instance = this;
            HarmonyInstance harmony = HarmonyInstance.Create(this.Helper.ModRegistry.ModID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            helper.Events.GameLoop.Saving += this.GameLoop_Saving;
            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;
        }

        internal bool IsDoorCollisionAt(GameLocation location, Rectangle position)
        {
            return this.manager.IsDoorCollisionAt(location, position);
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.manager.Reset();
            this.mapModifier.Reset();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button.IsActionButton() || e.Button.IsUseToolButton())
                this.manager.TryToggleDoor(Game1.currentLocation, new Point(Game1.player.getTileX(), Game1.player.getTileY()));
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.IsMultipleOf(3))
            {
                this.timer.TimeElapsed(50);
            }
        }

        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            this.serializer.Save(this.manager.Doors);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!this.creator.FindDoors())
                return;

            this.assetLoader.SetTextures(this.generator.GenerateDoorSprites(this.loadedContentPacks));
            IDictionary<GameLocation, IList<Door>> doors = this.creator.CreateDoors();
            this.creator.Reset();
            this.spriteManager.Reset();

            this.mapModifier.AddTileSheetsToMaps(doors);
            this.manager.Init(doors, this.serializer.Load() ?? new Dictionary<string, IDictionary<Point, State>>());
        }
    }
}