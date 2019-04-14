using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
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

        public DoorPositionSerializer(IDataHelper dataHelper)
        {
            this.dataHelper = dataHelper;
        }

        public void Save(Dictionary<string, Dictionary<Point, State>> doorsByLocation)
        {
            this.dataHelper.WriteSaveData(DoorPositionSerializer.DoorPositionKey, doorsByLocation);
        }

        public IDictionary<string, IDictionary<Point, State>> Load()
        {
            return this.dataHelper.ReadSaveData<IDictionary<string, IDictionary<Point, State>>>(DoorPositionSerializer.DoorPositionKey) ?? new Dictionary<string, IDictionary<Point, State>>();
        }
    }
}
