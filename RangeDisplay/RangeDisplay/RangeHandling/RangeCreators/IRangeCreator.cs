using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace RangeDisplay.RangeHandling.RangeCreators
{
    internal interface IRangeCreator : IHandlesRangeItem
    {
        IEnumerable<Vector2> GetRange(SObject obj, Vector2 position, GameLocation location);
    }
}