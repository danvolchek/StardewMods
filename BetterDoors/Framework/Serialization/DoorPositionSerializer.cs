using System;
using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace BetterDoors.Framework.Serialization
{
    /// <summary>
    /// Serializes door positions.
    /// </summary>
    internal class DoorPositionSerializer
    {
        internal const string DoorPositionKey = "door-positions";
        private readonly IDataHelper dataHelper;

        private Action<IDictionary<string, IDictionary<Point, State>>> pendingAction;

        public DoorPositionSerializer(IDataHelper dataHelper)
        {
            this.dataHelper = dataHelper;
        }

        public void Save(IDictionary<GameLocation, IList<Door>> doorsByLocation)
        {
            if (!Context.IsMainPlayer)
                return;
            IDictionary<string, IDictionary<Point, State>> doorPositions = new Dictionary<string, IDictionary<Point, State>>();

            foreach (KeyValuePair<GameLocation, IList<Door>> doorsInLocation in doorsByLocation)
            {
                doorPositions[doorsInLocation.Key.Name] = new Dictionary<Point, State>();
                foreach (Door door in doorsInLocation.Value)
                    doorPositions[doorsInLocation.Key.Name][door.Position] = door.State;
            }

            this.dataHelper.WriteSaveData(DoorPositionSerializer.DoorPositionKey, doorPositions);
        }

        public void OnLoad(Action<IDictionary<string, IDictionary<Point, State>>> action)
        {
            if (Context.IsMainPlayer)
            {
                action(this.dataHelper.ReadSaveData<IDictionary<string, IDictionary<Point, State>>>(DoorPositionSerializer.DoorPositionKey));
            }
            else
            {
                this.pendingAction = action;
            }
        }

        public void ReceivedSaveData(IDictionary<string, IDictionary<Point, State>> data)
        {
            this.pendingAction?.Invoke(data);
        }
    }
}
