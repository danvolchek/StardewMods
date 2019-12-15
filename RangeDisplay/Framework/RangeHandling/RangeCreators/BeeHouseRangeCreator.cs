using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace RangeDisplay.Framework.RangeHandling.RangeCreators
{
    /// <summary>Creates range for bee houses.</summary>
    internal class BeeHouseRangeCreator : IRangeCreator<SObject>
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The handled range item.</summary>
        public RangeItem HandledRangeItem => RangeItem.BeeHouse;

        /*********
        ** Public methods
        *********/

        /// <summary>Whether this creator can create range for the given object.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>Whether range can be created for it.</returns>
        public bool CanCreateRangeFor(SObject obj)
        {
            return obj.name.ToLower().Contains("bee house");
        }

        /// <summary>Creates a range.</summary>
        /// <param name="obj">The object to create a range for.</param>
        /// <param name="position">The position the object is at.</param>
        /// <param name="location">The location the object is in.</param>
        /// <returns>The created range.</returns>
        public IEnumerable<Vector2> CreateRange(SObject obj, Vector2 position, GameLocation location)
        {
            for (int i = -5; i < 6; i++)
                for (int j = -4; j < 5; j++)
                    if (Math.Abs(i) + Math.Abs(j) < 6)
                        yield return new Vector2(position.X + i, position.Y + j);
        }
    }
}
