﻿using BetterDoors.Framework.DoorGeneration;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Mapping;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace BetterDoors.Framework
{
    /// <summary>
    /// A door that can be opened and closed.
    /// </summary>
    internal class Door
    {
        public State State
        {
            get => this.state;
            set
            {
                this.state = value;
                this.SetState();
            }
        }

        public Point Position { get; }
        public GeneratedDoorTileInfo DoorTileInfo { get; }
        public Rectangle CollisionInfo { get; }
        public MapDoorExtraProperty Extras { get; }

        private readonly Map map;
        private readonly CallbackTimer timer;
        public Orientation Orientation { get; }

        private State stateBeforeToggle;
        private State state;

        public Door(Point position, Orientation orientation, MapDoorExtraProperty extras, Map map, GeneratedDoorTileInfo doorTileInfo, CallbackTimer timer)
        {
            this.Position = position;
            this.Orientation = orientation;
            this.Extras = extras;
            this.map = map;
            this.DoorTileInfo = doorTileInfo;
            this.timer = timer;

            this.CollisionInfo = new Rectangle(this.Position.X * 64 + this.DoorTileInfo.CollisionInfo.X, this.Position.Y * 64 + this.DoorTileInfo.CollisionInfo.Y, this.DoorTileInfo.CollisionInfo.Width, this.DoorTileInfo.CollisionInfo.Height);
        }

        public bool Toggle(bool force)
        {
            if (!force && this.timer.IsRegistered(this.ToggleCallback))
                return false;

            Game1.currentLocation.playSoundAt(this.State == State.Open ? "doorCreak" : "doorOpen", new Vector2(this.Position.X, this.Position.Y));

            this.stateBeforeToggle = this.State;
            this.timer.RegisterCallback(this.ToggleCallback, 0);

            return true;
        }

        private void SetState()
        {
            int topTileIndex = this.DoorTileInfo.TopLeftTileIndex + this.DoorTileInfo.GetTileIndex(this.State);
            int middleTileIndex = topTileIndex + this.DoorTileInfo.TileSheetInfo.TileSheetDimensions.X;
            int bottomTileIndex = middleTileIndex + this.DoorTileInfo.TileSheetInfo.TileSheetDimensions.X;

            Layer buildings = this.map.GetLayer("Buildings");
            Layer front = this.map.GetLayer("Front");
            TileSheet tileSheet = this.map.GetTileSheet(this.DoorTileInfo.TileSheetInfo.TileSheetId);

            buildings.Tiles[this.Position.X, this.Position.Y] = new StaticTile(buildings, tileSheet, BlendMode.Alpha, bottomTileIndex);
            front.Tiles[this.Position.X, this.Position.Y - 1] = new StaticTile(front, tileSheet, BlendMode.Alpha, middleTileIndex);
            front.Tiles[this.Position.X, this.Position.Y - 2] = new StaticTile(front, tileSheet, BlendMode.Alpha, topTileIndex);

            // Display fixes for horizontal doors
            if (this.Orientation == Orientation.Horizontal)
            {
                Layer alwaysFront = this.map.GetLayer("AlwaysFront");
                // Fix the player occluding the top tile when they walk by the door. There (should) be a wall there anyway? Perhaps make a config option.
                alwaysFront.Tiles[this.Position.X, this.Position.Y - 2] = new StaticTile(alwaysFront, tileSheet, BlendMode.Alpha, topTileIndex);
                // Fix the player occluding the bottom tile when they walk by the door.
                front.Tiles[this.Position.X, this.Position.Y] = new StaticTile(front, tileSheet, BlendMode.Alpha, bottomTileIndex);
            }

            if (this.Orientation == Orientation.Vertical && this.State != State.Closed)
            {
                front.Tiles[this.Position.X, this.Position.Y - 1] = null;
                buildings.Tiles[this.Position.X, this.Position.Y - 1] = new StaticTile(buildings, tileSheet, BlendMode.Alpha, middleTileIndex);
            }
        }

        public void RemoveFromMap()
        {
            Layer buildings = this.map.GetLayer("Buildings");
            Layer front = this.map.GetLayer("Front");
            Layer alwaysFront = this.map.GetLayer("AlwaysFront");

            foreach(Layer layer in new[] { buildings, front, alwaysFront })
            {
                for(int i = 0; i < 3; i++)
                {
                    Tile tile = layer.Tiles[this.Position.X, this.Position.Y - i];
                    if (tile != null && tile.DependsOnTileSheet(this.map.GetTileSheet(this.DoorTileInfo.TileSheetInfo.TileSheetId)))
                        layer.Tiles[this.Position.X, this.Position.Y - i] = null;
                }
            }
        }

        private int ToggleCallback()
        {
            State oldState = this.State;
            this.State = this.GetNextState();

            if (this.State != oldState)
            {
                return 100;
            }

            return -1;
        }

        private State GetNextState()
        {
            bool opening = this.stateBeforeToggle == State.Closed;
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