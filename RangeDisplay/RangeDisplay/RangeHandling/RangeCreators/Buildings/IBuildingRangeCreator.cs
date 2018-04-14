using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace RangeDisplay.RangeHandling.RangeCreators
{
    internal interface IBuildingRangeCreator : IHandlesRangeItem
    {
        IEnumerable<Vector2> GetForceRange(Vector2 position, GameLocation location);

        IEnumerable<Vector2> GetRange(Vector2 position, GameLocation location);
    }
}