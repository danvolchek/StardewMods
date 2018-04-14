using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace RangeDisplay.RangeHandling.RangeCreators
{
    internal class BeeHouseRangeCreator : IObjectRangeCreator
    {
        public RangeItem HandledRangeItem => RangeItem.Bee_House;

        public bool CanHandle(SObject obj)
        {
            return obj.name.ToLower().Contains("bee house");
        }

        public IEnumerable<Vector2> GetRange(SObject obj, Vector2 position, GameLocation location)
        {
            for (int i = -5; i < 6; i++)
                for (int j = -4; j < 5; j++)
                    if (Math.Abs(i) + Math.Abs(j) < 6)
                        yield return new Vector2(position.X + i, position.Y + j);
        }
    }
}