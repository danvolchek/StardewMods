using System.Collections.Generic;
using BetterDoors.Framework;
using BetterDoors.Framework.ContentPacks;
using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Mapping;
using BetterDoors.Framework.Serialization;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BetterDoors
{
    //TODO:
    // - Sound when opening/closing
    // - Double doors
    // - Vertical doors
    //    - They stop you from walking before you're close enough to them in one direction. Feasible to fix?
    // - There's one more axis the doors could theoretically be opened on - decide whether it's feasible/worth it to add.
    // - Multiplayer

    /// <summary>
    /// A mod which provides better doors to map makers.
    /// </summary>
    public class BetterDoorsMod : Mod
    {
        private DoorPositionSerializer serializer;
        private CallbackTimer timer;
        private DoorManager manager;

        public override void Entry(IModHelper helper)
        {
            this.serializer = new DoorPositionSerializer(this.Helper.Data);
            this.timer = new CallbackTimer();

            // Load content packs.
            IList<LoadedContentPackDoorEntry> loadedDoorEntries = new ContentPackLoader(helper, this.Monitor).LoadContentPacks();
            // Generate sprites.
            GeneratedSpriteManager spriteManager = new DoorSpriteGenerator(new DoorAssetLoader(helper.Content), this.Monitor, Game1.graphics.GraphicsDevice).GenerateDoorSprites(loadedDoorEntries);
            // Construct door manager.
            this.manager = new DoorManager(new DoorCreator(spriteManager, this.timer, this.Monitor), new MapModifier());

            helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            helper.Events.GameLoop.Saving += this.GameLoop_Saving;
            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
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
            //Start the entire door loading process, eventually setting the position of the doors to what they were before.
            this.manager.Init(this.serializer.Load() ?? new Dictionary<string, IDictionary<Point, State>>());
        }
    }
}