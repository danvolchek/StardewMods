using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace RangeDisplay.RangeHandling.RangeCreators.Buildings
{
    internal class JunimoHutRangeCreator : IBuildingRangeCreator
    {
        public RangeItem HandledRangeItem => RangeItem.Junimo_Hut;

        public IEnumerable<Vector2> GetForceRange(Vector2 position, GameLocation location)
        {
            if (location is BuildableGameLocation buildable)
                foreach (JunimoHut hut in buildable.buildings.OfType<JunimoHut>())
                    if (new Rectangle(hut.tileX.Value, hut.tileY.Value, hut.tilesWide.Value, hut.tilesHigh.Value).Contains((int)position.X, (int)position.Y))
                        for (int x = -8; x < 9; x++)
                            for (int y = -8; y < 9; y++)
                                yield return new Vector2(hut.tileX.Value + 1 + x, hut.tileY.Value + 1 + y);
        }

        public IEnumerable<Vector2> GetRange(Vector2 position, GameLocation location)
        {
            if (location is BuildableGameLocation buildable)
                foreach (JunimoHut hut in buildable.buildings.OfType<JunimoHut>())
                    if (!new Rectangle(hut.tileX.Value, hut.tileY.Value, hut.tilesWide.Value, hut.tilesHigh.Value).Contains((int)position.X, (int)position.Y))
                        for (int x = -8; x < 9; x++)
                            for (int y = -8; y < 9; y++)
                                yield return new Vector2(hut.tileX.Value + 1 + x, hut.tileY.Value + 1 + y);
        }
    }
}