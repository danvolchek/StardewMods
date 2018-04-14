using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace RangeDisplay.RangeHandling.RangeCreators
{
    internal class SprinklerRangeCreator : IObjectRangeCreator
    {
        public RangeItem HandledRangeItem => RangeItem.Sprinkler;

        public bool CanHandle(SObject obj)
        {
            return obj.name.ToLower().Contains("sprinkler");
        }

        public IEnumerable<Vector2> GetRange(SObject obj, Vector2 position, GameLocation location)
        {
            string objectName = obj.Name.ToLower();

            yield return new Vector2(position.X - 1, position.Y);
            yield return new Vector2(position.X + 1, position.Y);
            yield return new Vector2(position.X, position.Y - 1);
            yield return new Vector2(position.X, position.Y + 1);

            if (objectName.Contains("quality") || objectName.Contains("iridium"))
            {
                yield return new Vector2(position.X - 1, position.Y - 1);
                yield return new Vector2(position.X + 1, position.Y - 1);
                yield return new Vector2(position.X - 1, position.Y + 1);
                yield return new Vector2(position.X + 1, position.Y + 1);

                if (objectName.Contains("iridium"))
                {
                    for (int i = -2; i < 3; i++)
                    {
                        for (int j = -2; j < 3; j++)
                        {
                            if (Math.Abs(i) == 2 || Math.Abs(j) == 2)
                            {
                                yield return new Vector2(position.X + i, position.Y + j);
                            }
                        }
                    }
                }
            }
        }
    }
}