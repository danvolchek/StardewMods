using BetterDoors.Framework.Enums;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Collections.Generic;

namespace BetterDoors.Framework.Serialization
{
    /// <summary>Serializes door positions.</summary>
    internal class DoorPositionSerializer
    {
        /*********
        ** Fields
        *********/

        /// <summary>The key used to read and write data.</summary>
        internal const string DoorPositionKey = "door-positions";

        /// <summary>Provides an API for reading and storing local mod data.</summary>
        private readonly IDataHelper dataHelper;

        /*********
        ** Public methods
        *********/

        /// <summary>Constructs an instance.</summary>
        /// <param name="dataHelper">Provides an API for reading and storing local mod data.</param>
        public DoorPositionSerializer(IDataHelper dataHelper)
        {
            this.dataHelper = dataHelper;
        }

        /// <summary>Save states to the save file.</summary>
        /// <param name="doorsByLocation">The state to save.</param>
        public void Save(Dictionary<string, Dictionary<Point, State>> doorsByLocation)
        {
            this.dataHelper.WriteSaveData(DoorPositionSerializer.DoorPositionKey, doorsByLocation);
        }

        /// <summary>Read states from the save file.</summary>
        /// <returns>The read states.</returns>
        public IDictionary<string, IDictionary<Point, State>> Load()
        {
            return this.dataHelper.ReadSaveData<IDictionary<string, IDictionary<Point, State>>>(DoorPositionSerializer.DoorPositionKey) ?? new Dictionary<string, IDictionary<Point, State>>();
        }
    }
}
