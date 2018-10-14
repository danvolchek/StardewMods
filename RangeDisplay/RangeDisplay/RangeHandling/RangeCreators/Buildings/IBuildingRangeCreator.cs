using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace RangeDisplay.RangeHandling.RangeCreators.Buildings
{
    internal interface IBuildingRangeCreator : IHandlesRangeItem
    {
        IEnumerable<Vector2> GetForceRange(Vector2 position, GameLocation location);

        IEnumerable<Vector2> GetRange(Vector2 position, GameLocation location);
    }
}