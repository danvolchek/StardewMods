using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using System.Collections.Generic;

namespace RangeDisplay.Framework.RangeHandling.RangeCreators
{
    /// <summary>Creates range for junimo huts.</summary>
    internal class JunimoHutRangeCreator : IRangeCreator<Building>
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The handled range item.</summary>
        public RangeItem HandledRangeItem => RangeItem.JunimoHut;

        /*********
        ** Public methods
        *********/

        /// <summary>Whether this creator can create range for the given object.</summary>
        /// <param name="building">The object.</param>
        /// <returns>Whether range can be created for it.</returns>
        public bool CanCreateRangeFor(Building building)
        {
            return building is JunimoHut;
        }

        /// <summary>Creates a range.</summary>
        /// <param name="building">The object to create a range for.</param>
        /// <param name="position">The position the object is at.</param>
        /// <param name="location">The location the object is in.</param>
        /// <returns>The created range.</returns>
        public IEnumerable<Vector2> CreateRange(Building building, Vector2 position, GameLocation location)
        {
            for (int x = -8; x < 9; x++)
            {
                for (int y = -8; y < 9; y++)
                {
                    yield return new Vector2(building.tileX.Value + 1 + x, building.tileY.Value + 1 + y);
                }
            }
        }
    }
}
