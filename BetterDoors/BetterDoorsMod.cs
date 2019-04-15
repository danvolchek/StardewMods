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
             - There's one more axis the doors could theoretically be opened on - decide whether it's feasible/worth it to add. -> A later release.
         - Code Review:
             - Think about how door states are synced in multiplayer and whether a desync could happen.
     - UX:
         - Write up documentation.
    */

    /// <summary> A mod which provides better doors to map makers.</summary>
    public class BetterDoorsMod : Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>Handles reading and writing door state data from the save file.</summary>
        private DoorPositionSerializer serializer;

        /// <summary>Notifies doors about the time in fixed intervals.</summary>
        private CallbackTimer timer;

        /// <summary>Holds info about generated tile sheets.</summary>
        private GeneratedDoorTileInfoManager doorTileInfoManager;

        /// <summary>Generates tile sheets.</summary>
        private DoorSpriteGenerator generator;

        /// <summary>Adds tile sheets to SMAPI's content loaders.</summary>
        private DoorAssetLoader assetLoader;

        /// <summary>Manages map tile sheets.</summary>
        private MapTileSheetManager mapTileSheetManager;

        /// <summary>Finds and creates doors.</summary>
        private DoorCreator creator;

        /// <summary>Manages created doors.</summary>
        private DoorManager manager;

        /// <summary>Whether the mod is enabled or not.</summary>
        /// <remarks>See <see cref="Multiplayer_PeerContextReceived"/> for why it would be disabled.</remarks>
        private bool enabled;

        /*********
        ** Accessors
        *********/
        /// <summary>A static reference to the mod for Harmony patches to use.</summary>
        internal static BetterDoorsMod Instance { get; private set; }

        /*********
         ** Public methods
         *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Create helper classes that make the mod work.
            ErrorQueue errorQueue = new ErrorQueue(this.Monitor);
            ContentPackLoader packLoader = new ContentPackLoader(this.Helper, this.Monitor, errorQueue);

            this.serializer = new DoorPositionSerializer(this.Helper.Data);
            this.timer = new CallbackTimer();
            this.doorTileInfoManager = new GeneratedDoorTileInfoManager();
            this.generator = new DoorSpriteGenerator(this.doorTileInfoManager, packLoader.LoadContentPacks(), this.Helper.Multiplayer.ModID, this.Monitor, Game1.graphics.GraphicsDevice);
            this.creator = new DoorCreator(this.doorTileInfoManager, this.timer, errorQueue, this.Helper.ModRegistry.Get(this.Helper.ModRegistry.ModID).Manifest.Version);
            this.assetLoader = new DoorAssetLoader(this.Helper.Content);
            this.mapTileSheetManager = new MapTileSheetManager();
            this.manager = new DoorManager(this.OnDoorToggled);

            // Apply Harmony patches.
            BetterDoorsMod.Instance = this;
            HarmonyInstance harmony = HarmonyInstance.Create(this.Helper.ModRegistry.ModID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Attach events.
            this.Enable();
            this.Helper.Events.Multiplayer.PeerContextReceived += this.Multiplayer_PeerContextReceived;
            this.Helper.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;
        }

        /*********
         ** Private methods
         *********/
        /// <summary>Enables the mod, attaching necessary event handlers.</summary>
        private void Enable()
        {
            if (this.enabled)
                return;

            this.enabled = true;

            this.Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            this.Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            this.Helper.Events.GameLoop.Saving += this.GameLoop_Saving;
            this.Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            this.Helper.Events.Player.Warped += this.Player_Warped;
            this.Helper.Events.Multiplayer.ModMessageReceived += this.Multiplayer_ModMessageReceived;
        }

        /// <summary>Disables the mod, removing the attached event handlers.</summary>
        private void Disable()
        {
            if (!this.enabled)
                return;

            this.enabled = false;

            this.Helper.Events.Input.ButtonPressed -= this.Input_ButtonPressed;
            this.Helper.Events.GameLoop.UpdateTicked -= this.GameLoop_UpdateTicked;
            this.Helper.Events.GameLoop.Saving -= this.GameLoop_Saving;
            this.Helper.Events.GameLoop.SaveLoaded -= this.GameLoop_SaveLoaded;
            this.Helper.Events.Player.Warped -= this.Player_Warped;
            this.Helper.Events.Multiplayer.ModMessageReceived -= this.Multiplayer_ModMessageReceived;
        }

        /*********
         ** Event Handlers
         *********/
        /// <summary>Raised after the mod context for a peer is received. This happens before the game approves the connection, so the player doesn't yet exist in the game. This is the earliest point where messages can be sent to the peer via SMAPI.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Multiplayer_PeerContextReceived(object sender, PeerContextReceivedEventArgs e)
        {
            // If the host doesn't have the same version of Better Doors installed, disable for this player.
            if (e.Peer.IsHost && (!e.Peer.HasSmapi || e.Peer.GetMod(this.Helper.Multiplayer.ModID)?.Version != this.Helper.ModRegistry.Get(this.Helper.Multiplayer.ModID).Manifest.Version))
                this.Disable();
        }

        /// <summary>Raised after the game returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.Enable();
            this.Reset();
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || (!e.Button.IsActionButton() && !e.Button.IsUseToolButton()))
                return;

            this.manager.FuzzyToggleDoor(Utils.GetLocationName(Game1.currentLocation), new Point((int)e.Cursor.Tile.X, (int)e.Cursor.Tile.Y));
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.IsMultipleOf(3))
            {
                this.timer.TimeElapsed(50);
            }

            if (e.IsMultipleOf(10))
            {
                this.manager.ToggleAutomaticDoors(Game1.currentLocation);
            }
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            this.serializer.Save(this.manager.GetEveryDoorState());
        }

        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                // Load initial door states and prepare every location for the host.
                IDictionary<string, IDictionary<Point, State>> saveData = this.serializer.Load();
                foreach (GameLocation location in BetterDoorsMod.GetAllLocations())
                {
                    if(this.PrepareLocation(location))
                        this.manager.SetDoorStates(Utils.GetLocationName(location), saveData.TryGetValue(Utils.GetLocationName(location), out IDictionary<Point, State> doorStates) ? doorStates : null);
                }
            }
            else
            {
                // Only prepare the current location for farmhands. Also request initial door states.
                this.PrepareLocation(Game1.currentLocation);
                this.Helper.Multiplayer.SendMessage(new DoorStateRequest(Utils.GetLocationName(Game1.currentLocation)), nameof(DoorStateRequest), new[] { this.Helper.Multiplayer.ModID });
            }
        }

        /// <summary>Raised after a player warps to a new location. NOTE: this event is currently only raised for the current player.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                // Prepare the current location for farmhands. Also request initial door states.
                this.Reset();
                this.PrepareLocation(e.NewLocation);
                this.Helper.Multiplayer.SendMessage(new DoorStateRequest(Utils.GetLocationName(e.NewLocation)), nameof(DoorStateRequest), new[] { this.Helper.Multiplayer.ModID });
            }

            this.manager.ResetAutomaticDoorTracking();
        }

        /// <summary>Raised after a mod message is received over the network.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if(e.FromModID != this.Helper.Multiplayer.ModID)
                return;

            // Upon receiving a door state requests as the host with the current state of the doors.
            if(e.Type == nameof(DoorStateRequest) && Context.IsMainPlayer)
            {
                DoorStateRequest request = e.ReadAs<DoorStateRequest>();
                this.Helper.Multiplayer.SendMessage(new DoorStateReply(request.LocationName, this.manager.GetDoorStatesInLocation(request.LocationName)), nameof(DoorStateReply), new []{this.Helper.Multiplayer.ModID}, new []{e.FromPlayerID});
            }

            // Upon receiving a door state reply as a farmhand, update the state of the doors.
            if (e.Type == nameof(DoorStateReply) && !Context.IsMainPlayer)
            {
                DoorStateReply reply = e.ReadAs<DoorStateReply>();
                this.manager.SetDoorStates(reply.LocationName, reply.DoorStates);
            }

            // Upon receiving a door toggle, update the door if the location is loaded.
            if (e.Type == nameof(DoorToggle))
            {
                DoorToggle toggle = e.ReadAs<DoorToggle>();
                if(Context.IsMainPlayer || toggle.LocationName == Utils.GetLocationName(Game1.currentLocation))
                    this.manager.ToggleDoor(toggle.LocationName, toggle.Position);
            }
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Action to take when a door is toggled.</summary>
        /// <param name="door">The door that was toggled.</param>
        private void OnDoorToggled(Door door)
        {
            // Broadcast the toggle to all other players.
            this.Helper.Multiplayer.SendMessage(new DoorToggle(door.Position, Utils.GetLocationName(Game1.currentLocation)), nameof(DoorToggle), new[] { this.Helper.Multiplayer.ModID });
        }

        /// <summary>Prepare a location for displaying doors.</summary>
        /// <param name="location">The location to prepare.</param>
        /// <returns>Whether any doors were added to the location.</returns>
        private bool PrepareLocation(GameLocation location)
        {
            // Don't reprocess the same location twice.
            if (this.manager.WasProcessed(Utils.GetLocationName(location)))
                return false;

            IList<Door> doors = new List<Door>();

            // Find doors in the location.
            if (this.creator.FindDoorsInLocation(location.map, out IList<PendingDoor> pendingDoors))
            {
                // Generate tile sheets that have the door sprites for the location and add them to SMAPI's content loader.
                this.assetLoader.AddTextures(this.generator.GenerateDoorSprites(Utils.GetLocationName(location)));
                // Create the found doors.
                doors = this.creator.CreateDoors(pendingDoors);
                // Add the generated tile sheets to the map.
                this.mapTileSheetManager.AddTileSheetsToMap(location.Map, doors);

                // Reset the sprite manager for future location preparation.
                this.doorTileInfoManager.Reset();
            }

            // Attach the doors to the location.
            this.manager.ManageDoors(Utils.GetLocationName(location), doors);

            return doors.Count != 0;
        }

        /// <summary>Reset the mod state for future save loads.</summary>
        private void Reset()
        {
            this.manager.Reset();
            this.mapTileSheetManager.Reset();
            this.assetLoader.Reset();
        }

        /// <summary>Gets all locations in the game.</summary>
        /// <returns>All the locations in the game.</returns>
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

        /// <summary>Checks for a closed door.</summary>
        /// <param name="location">The location to check in.</param>
        /// <param name="position">The position to check at.</param>
        /// <returns>Whether a closed door was found.</returns>
        internal bool IsClosedDoorAt(GameLocation location, Rectangle position)
        {
            return this.manager.IsClosedDoorAt(Utils.GetLocationName(location), position);
        }
    }
}