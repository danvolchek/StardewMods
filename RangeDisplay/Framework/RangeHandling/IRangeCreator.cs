using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace RangeDisplay.Framework.RangeHandling
{
    /// <summary>Handles a rage item.</summary>
    internal interface IRangeCreator<in T>
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The handled range item.</summary>
        RangeItem HandledRangeItem { get; }

        /// <summary>Whether this creator can create range for the given object.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>Whether range can be created for it.</returns>
        bool CanCreateRangeFor(T obj);

        /// <summary>Creates a range.</summary>
        /// <param name="obj">The object to create a range for.</param>
        /// <param name="position">The position the object is at.</param>
        /// <param name="location">The location the object is in.</param>
        /// <returns>The created range.</returns>
        IEnumerable<Vector2> CreateRange(T obj, Vector2 position, GameLocation location);
    }
}
