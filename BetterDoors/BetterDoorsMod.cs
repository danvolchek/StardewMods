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
using System.Collections.Generic;
using System.Reflection;

namespace BetterDoors
{
    /*TODO:
     - Double doors
     - There's one more axis the doors could theoretically be opened on - decide whether it's feasible/worth it to add.
     - Multiplayer
        On a warp:
            - Look for doors, generate a tilesheet, attach the sheet, create doors, init positions

            - If the doors/tilesheet are already there or attached, skip all that and just init positions. (except for the main player)

            - Questions:
                - Do tiles stick around for farmhands after they leave the location or are they reset
                - Do tilesheets stick around for farmhands after they leave the location or are they reset

            - Downsides:
                - New tilesheet generation per location load. Perhaps do everything in advance for the main player, and only over complicate for farmhands.
    */

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
            helper.Events.Multiplayer.ModMessageReceived += this.Multiplayer_ModMessageReceived;
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

            Point playerPos = new Point(Game1.player.getTileX(), Game1.player.getTileY());

            if (e.Button.IsActionButton() || e.Button.IsUseToolButton())
                this.manager.TryToggleDoor(Game1.currentLocation, playerPos);
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
            this.serializer.OnLoad(data =>
            {
                if(Context.IsMainPlayer)
                    this.Helper.Multiplayer.SendMessage(data, DoorPositionSerializer.DoorPositionKey, new []{this.Helper.Multiplayer.ModID});

                this.manager.Init(doors, data ?? new Dictionary<string, IDictionary<Point, State>>());
            });
        }

        private void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if(e.FromModID != this.Helper.Multiplayer.ModID)
                return;

            if(e.Type == DoorPositionSerializer.DoorPositionKey)
                this.serializer.ReceivedSaveData(e.ReadAs<IDictionary<string, IDictionary<Point, State>>>());
        }
    }
}