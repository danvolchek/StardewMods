using System;
using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using BetterDoors.Framework.Multiplayer.Messages;

namespace BetterDoors.Framework.Serialization
{
    /// <summary>
    /// Serializes door positions.
    /// </summary>
    internal class DoorPositionSerializer
    {
        internal const string DoorPositionKey = "door-positions";
        private readonly IDataHelper dataHelper;

        public DoorPositionSerializer(IDataHelper dataHelper)
        {
            this.dataHelper = dataHelper;
        }

        public void Save(IDictionary<string, IList<Door>> doorsByLocation)
        {
            if (!Context.IsMainPlayer)
                return;

            IDictionary<string, IDictionary<Point, State>> doorPositions = new Dictionary<string, IDictionary<Point, State>>();

            foreach (KeyValuePair<string, IList<Door>> doorsInLocation in doorsByLocation)
            {
                doorPositions[doorsInLocation.Key] = new Dictionary<Point, State>();
                foreach (Door door in doorsInLocation.Value)
                    doorPositions[doorsInLocation.Key][door.Position] = door.State;
            }

            this.dataHelper.WriteSaveData(DoorPositionSerializer.DoorPositionKey, doorPositions);
        }

        public IDictionary<string, IDictionary<Point, State>> Load()
        {
            return this.dataHelper.ReadSaveData<IDictionary<string, IDictionary<Point, State>>>(DoorPositionSerializer.DoorPositionKey) ?? new Dictionary<string, IDictionary<Point, State>>();
        }
    }
}
