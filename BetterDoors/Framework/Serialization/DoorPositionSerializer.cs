using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;

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

        public void Save(IDictionary<string, IDictionary<Point, Door>> doorsByLocation)
        {
            if (!Context.IsMainPlayer)
                return;

            this.dataHelper.WriteSaveData(DoorPositionSerializer.DoorPositionKey, doorsByLocation.ToDictionary(item => item.Key, item => item.Value.ToDictionary(item2 => item2.Key, item2 => item2.Value.State)));
        }

        public IDictionary<string, IDictionary<Point, State>> Load()
        {
            return this.dataHelper.ReadSaveData<IDictionary<string, IDictionary<Point, State>>>(DoorPositionSerializer.DoorPositionKey) ?? new Dictionary<string, IDictionary<Point, State>>();
        }
    }
}
