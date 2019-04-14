using BetterDoors.Framework;
using BetterDoors.Framework.ContentPacks;
using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Mapping;
using BetterDoors.Framework.Multiplayer;
using BetterDoors.Framework.Serialization;
using BetterDoors.Framework.Utility;
using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterDoors
{
    /*TODO:
     - Programming:
         - Features:
             - Automatic doors
             - There's one more axis the doors could theoretically be opened on - decide whether it's feasible/worth it to add. -> A later release.
         - Code Review:
             - Think about how map door properties are parsed and if it could be done better.
             - Think about how door states are synced in multiplayer and whether a desync could happen.
     - UX:
         - Go through all of the user input validation and make sure the errors are helpful.
         - Write up documentation.
    */

    /// <summary>
    /// A mod which provides better doors to map makers.
    /// </summary>
    public class BetterDoorsMod : Mod
    {
        private DoorPositionSerializer serializer;
        private CallbackTimer timer;

        private GeneratedSpriteManager spriteManager;
        private DoorSpriteGenerator generator;
        private DoorAssetLoader assetLoader;
        private MapModifier mapModifier;
        private DoorCreator creator;
        private DoorManager manager;

        private bool enabled;

        internal static BetterDoorsMod Instance { get; private set; }

        public override void Entry(IModHelper helper)
        {
            this.serializer = new DoorPositionSerializer(this.Helper.Data);
            this.timer = new CallbackTimer();

            this.spriteManager = new GeneratedSpriteManager();
            this.generator = new DoorSpriteGenerator(this.spriteManager, this.Helper.Multiplayer.ModID, this.Monitor, Game1.graphics.GraphicsDevice, new ContentPackLoader(this.Helper, this.Monitor).LoadContentPacks());
            this.creator = new DoorCreator(this.spriteManager, this.timer, this.Monitor);
            this.assetLoader = new DoorAssetLoader(this.Helper.Content);
            this.mapModifier = new MapModifier();
            this.manager = new DoorManager(this.Monitor);

            BetterDoorsMod.Instance = this;
            HarmonyInstance harmony = HarmonyInstance.Create(this.Helper.ModRegistry.ModID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            this.Enable();
            this.Helper.Events.Multiplayer.PeerContextReceived += this.Multiplayer_PeerContextReceived;
            this.Helper.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;
        }

        private void Enable()
        {
            if (this.enabled)
                return;
            this.enabled = true;

            this.Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            this.Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            this.Helper.Events.GameLoop.Saving += this.GameLoop_Saving;
            this.Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            this.Helper.Events.Multiplayer.ModMessageReceived += this.Multiplayer_ModMessageReceived;
            this.Helper.Events.Player.Warped += this.Player_Warped;
        }

        private void Disable()
        {
            if (!this.enabled)
                return;
            this.enabled = false;

            this.Helper.Events.Input.ButtonPressed -= this.Input_ButtonPressed;
            this.Helper.Events.GameLoop.UpdateTicked -= this.GameLoop_UpdateTicked;
            this.Helper.Events.GameLoop.Saving -= this.GameLoop_Saving;
            this.Helper.Events.GameLoop.SaveLoaded -= this.GameLoop_SaveLoaded;
            this.Helper.Events.GameLoop.ReturnedToTitle -= this.GameLoop_ReturnedToTitle;
            this.Helper.Events.Multiplayer.ModMessageReceived -= this.Multiplayer_ModMessageReceived;
            this.Helper.Events.Player.Warped -= this.Player_Warped;
        }

        private bool InitializeLocation(GameLocation location)
        {
            if (this.manager.WasProcessed(Utils.GetLocationName(location)))
                return false;

            IList<Door> doors = new List<Door>();

            if (this.creator.FindDoorsInLocation(location))
            {
                this.assetLoader.AddTextures(this.generator.GenerateDoorSprites(Utils.GetLocationName(location)));
                doors = this.creator.CreateDoors();
                this.creator.Reset();
                this.spriteManager.Reset();

                this.mapModifier.AddTileSheetsToLocation(location.Map, doors);
            }

            this.manager.AddDoorsToLocation(Utils.GetLocationName(location), doors);

            return doors.Count != 0;
        }

        private void Multiplayer_PeerContextReceived(object sender, PeerContextReceivedEventArgs e)
        {
            if (e.Peer.IsHost && (!e.Peer.HasSmapi || e.Peer.GetMod(this.Helper.Multiplayer.ModID)?.Version != this.Helper.ModRegistry.Get(this.Helper.Multiplayer.ModID).Manifest.Version))
                this.Disable();
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.Enable();
            this.Reset();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || (!e.Button.IsActionButton() && !e.Button.IsUseToolButton()))
                return;

            Point position = new Point((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y);
            if (this.manager.TryToggleDoor(Utils.GetLocationName(Game1.currentLocation), position, out IList<Door> doors))
            {
                foreach(Door door in doors)
                    this.Helper.Multiplayer.SendMessage(new DoorToggle(door.Position, Utils.GetLocationName(Game1.currentLocation)), nameof(DoorToggle), new[] { this.Helper.Multiplayer.ModID });
            }
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
            this.serializer.Save(this.manager.GetDoors());
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                IDictionary<string, IDictionary<Point, State>> saveData = this.serializer.Load();
                foreach (GameLocation location in BetterDoorsMod.GetAllLocations())
                {
                    if(this.InitializeLocation(location))
                        this.manager.InitializeDoorStates(Utils.GetLocationName(location), saveData.TryGetValue(Utils.GetLocationName(location), out IDictionary<Point, State> doorStates) ? doorStates : null);
                }
            }
            else
            {
                this.InitializeLocation(Game1.currentLocation);
                this.Helper.Multiplayer.SendMessage(new DoorStateRequest(Utils.GetLocationName(Game1.currentLocation)), nameof(DoorStateRequest), new[] { this.Helper.Multiplayer.ModID });
            }
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                // Some locations stick around, but others don't. So we need to do a full reload of this location. Farmhands effectively only load doors in one location at a time
                // while the host loads all of them.
                this.Reset();
                this.InitializeLocation(e.NewLocation);
                this.Helper.Multiplayer.SendMessage(new DoorStateRequest(Utils.GetLocationName(e.NewLocation)), nameof(DoorStateRequest), new[] { this.Helper.Multiplayer.ModID });
            }
        }

        private void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if(e.FromModID != this.Helper.Multiplayer.ModID)
                return;

            if(e.Type == nameof(DoorStateRequest) && Context.IsMainPlayer)
            {
                DoorStateRequest request = e.ReadAs<DoorStateRequest>();
                this.Helper.Multiplayer.SendMessage(new DoorStateReply(request.LocationName, this.manager.GetStates(request.LocationName)), nameof(DoorStateReply), new []{this.Helper.Multiplayer.ModID}, new []{e.FromPlayerID});
            }

            if (e.Type == nameof(DoorStateReply) && !Context.IsMainPlayer)
            {
                DoorStateReply reply = e.ReadAs<DoorStateReply>();
                this.manager.InitializeDoorStates(reply.LocationName, reply.DoorStates);
            }

            if (e.Type == nameof(DoorToggle))
            {
                DoorToggle toggle = e.ReadAs<DoorToggle>();
                if(Context.IsMainPlayer || toggle.LocationName == Utils.GetLocationName(Game1.currentLocation))
                    this.manager.ToggleDoor(toggle.LocationName, toggle.Position);
            }
        }

        private void Reset()
        {
            this.manager.Reset();
            this.mapModifier.Reset();
            this.assetLoader.Reset();
        }

        private static IEnumerable<GameLocation> GetAllLocations()
        {
            foreach (GameLocation location in Game1.locations.Where(location => location != null))
            {
                yield return location;

                if (!(location is BuildableGameLocation bLoc))
                {
                    continue;
                }

                foreach (GameLocation buildingLoc in bLoc.buildings.Select(building => building.indoors.Value).Where(buildingLoc => buildingLoc != null))
                    yield return buildingLoc;
            }
        }

        internal bool IsDoorCollisionAt(GameLocation location, Rectangle position)
        {
            return this.manager.IsDoorCollisionAt(Utils.GetLocationName(location), position);
        }
    }
}