using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Mapping.Properties;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace BetterDoors.Framework
{
    /// <summary>A door that collides with players and can be opened and closed.</summary>
    internal class Door
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The state of the door.</summary>
        public State State
        {
            get => this.state;
            set
            {
                this.state = value;
                this.UpdateTiles();
            }
        }

        /// <summary>The position of the door.</summary>
        public Point Position { get; }

        /// <summary>The orientation of the door.</summary>
        public Orientation Orientation { get; }

        /// <summary>The door extras of the door.</summary>
        public MapDoorExtras Extras { get; }

        /// <summary>The generated tile info of the door.</summary>
        public GeneratedDoorTileInfo DoorTileInfo { get; }

        /// <summary>The collision info of the door.</summary>
        public Rectangle CollisionInfo { get; }

        /// <summary>The state before a toggle began.</summary>
        public State StateBeforeToggle { get; set; }

        /// <summary>Whether the door is currently animating.</summary>
        public bool IsAnimating => this.timer.IsRegistered(this.TimerCallback);

        /*********
        ** Fields
        *********/
        /// <summary>The map the door is in.</summary>
        private readonly Map map;

        /// <summary>A timer used to animate state change.</summary>
        private readonly CallbackTimer timer;

        /// <summary>The current state. Use <see cref="State"/> instead.</summary>
        private State state;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="position">The position of the door.</param>
        /// <param name="orientation">The orientation of the door.</param>
        /// <param name="extras">The door extras of the door.</param>
        /// <param name="map">The map the door is in.</param>
        /// <param name="doorTileInfo">The generated tile info of the door.</param>
        /// <param name="timer">A timer used to animate state change.</param>
        public Door(Point position, Orientation orientation, MapDoorExtras extras, Map map, GeneratedDoorTileInfo doorTileInfo, CallbackTimer timer)
        {
            this.Position = position;
            this.Orientation = orientation;
            this.Extras = extras;
            this.map = map;
            this.DoorTileInfo = doorTileInfo;
            this.timer = timer;

            this.CollisionInfo = new Rectangle(this.Position.X * 64 + this.DoorTileInfo.CollisionInfo.X, this.Position.Y * 64 + this.DoorTileInfo.CollisionInfo.Y, this.DoorTileInfo.CollisionInfo.Width, this.DoorTileInfo.CollisionInfo.Height);
        }

        /// <summary>Toggle this door's state.</summary>
        /// <param name="force">If true, cancel any current toggle animation. Otherwise do nothing.</param>
        /// <returns>Whether the toggle animation started.</returns>
        public bool Toggle(bool force)
        {
            if (!force && this.IsAnimating)
                return false;

            Game1.currentLocation.playSoundAt(this.State == State.Closed ? "doorCreak" : "doorOpen", new Vector2(this.Position.X, this.Position.Y));

            // If not toggling, the state before toggle is the current state. Otherwise, it's the opposite of the old state before toggle.
            if (!this.IsAnimating)
                this.StateBeforeToggle = this.State;
            else
                this.StateBeforeToggle = this.StateBeforeToggle == State.Closed ? State.Open : State.Closed;

            this.timer.RegisterCallback(this.TimerCallback, 0);
            return true;
        }

        /// <summary>Removes the door from the map its in.</summary>
        public void RemoveFromMap()
        {
            Layer buildings = this.map.GetLayer("Buildings");
            Layer front = this.map.GetLayer("Front");
            Layer alwaysFront = this.map.GetLayer("AlwaysFront");

            foreach (Layer layer in new[] { buildings, front, alwaysFront })
            {
                for (int i = 0; i < 3; i++)
                {
                    Tile tile = layer.Tiles[this.Position.X, this.Position.Y - i];
                    if (tile != null && tile.DependsOnTileSheet(this.map.GetTileSheet(this.DoorTileInfo.TileSheetInfo.TileSheetId)))
                        layer.Tiles[this.Position.X, this.Position.Y - i] = null;
                }
            }
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Updates map tiles, changing how the door appears.</summary>
        private void UpdateTiles()
        {
            // Get tile indices.
            int topTileIndex = this.DoorTileInfo.TopLeftTileIndex + this.DoorTileInfo.GetTileIndex(this.State);
            int middleTileIndex = topTileIndex + this.DoorTileInfo.TileSheetInfo.TileSheetDimensions.X;
            int bottomTileIndex = middleTileIndex + this.DoorTileInfo.TileSheetInfo.TileSheetDimensions.X;

            // Get layers in tile sheet.
            Layer buildings = this.map.GetLayer("Buildings");
            Layer front = this.map.GetLayer("Front");
            TileSheet tileSheet = this.map.GetTileSheet(this.DoorTileInfo.TileSheetInfo.TileSheetId);

            // Create new tiles.
            buildings.Tiles[this.Position.X, this.Position.Y] = new StaticTile(buildings, tileSheet, BlendMode.Alpha, bottomTileIndex);
            front.Tiles[this.Position.X, this.Position.Y - 1] = new StaticTile(front, tileSheet, BlendMode.Alpha, middleTileIndex);
            front.Tiles[this.Position.X, this.Position.Y - 2] = new StaticTile(front, tileSheet, BlendMode.Alpha, topTileIndex);

            // Display fixes for horizontal doors.
            if (this.Orientation == Orientation.Horizontal)
            {
                Layer alwaysFront = this.map.GetLayer("AlwaysFront");
                // Fix the player occluding the top tile when they walk by the door. There (should) be a wall there anyway? Perhaps make a config option.
                alwaysFront.Tiles[this.Position.X, this.Position.Y - 2] = new StaticTile(alwaysFront, tileSheet, BlendMode.Alpha, topTileIndex);
                // Fix the player occluding the bottom tile when they walk by the door.
                front.Tiles[this.Position.X, this.Position.Y] = new StaticTile(front, tileSheet, BlendMode.Alpha, bottomTileIndex);
            }

            // Display fixed for vertical non-closed doors.
            if (this.Orientation == Orientation.Vertical && this.State != State.Closed)
            {
                // Fix the player appear on top of the door when standing near the middle of it.
                front.Tiles[this.Position.X, this.Position.Y - 1] = null;
                buildings.Tiles[this.Position.X, this.Position.Y - 1] = new StaticTile(buildings, tileSheet, BlendMode.Alpha, middleTileIndex);
            }
        }

        /// <summary>Function to call when the timer fires.</summary>
        /// <returns></returns>
        private int TimerCallback()
        {
            // Move to next state in the animation.
            State oldState = this.State;
            this.State = this.GetNextStateInAnimation();

            // Continue animating if animation isn't done, otherwise stop.
            if (this.State != oldState)
            {
                return 100;
            }

            return -1;
        }

        /// <summary>Gets the next state in the animation.</summary>
        /// <returns>The next state.</returns>
        private State GetNextStateInAnimation()
        {
            bool opening = this.StateBeforeToggle == State.Closed;
            switch (this.state)
            {
                case State.Closed:
                    return opening ? State.SlightlyOpen : State.Closed;
                case State.SlightlyOpen:
                    return opening ? State.MostlyOpen : State.Closed;
                case State.MostlyOpen:
                    return opening ? State.Open : State.SlightlyOpen;
                case State.Open:
                    return opening ? State.Open : State.MostlyOpen;
                default:
                    return State.Open;
            }
        }
    }
}