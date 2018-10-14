using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace RangeDisplay.RangeHandling.RangeCreators.Objects
{
    internal class ScarecrowRangeCreator : IObjectRangeCreator
    {
        public RangeItem HandledRangeItem => RangeItem.Scarecrow;

        public bool CanHandle(SObject obj)
        {
            return obj.bigCraftable.Value && obj.name.ToLower().Contains("arecrow");
        }

        public IEnumerable<Vector2> GetRange(SObject obj, Vector2 position, GameLocation location)
        {
            for (int x = -9; x < 10; x++)
                for (int y = -9; y < 10; y++)
                {
                    Vector2 item = new Vector2(position.X + x, position.Y + y);
                    if (Vector2.Distance(item, position) < 9.0)
                        yield return item;
                }
        }
    }
}