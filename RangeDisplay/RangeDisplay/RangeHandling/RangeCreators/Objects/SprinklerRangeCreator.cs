using Microsoft.Xna.Framework;
using RangeDisplay.APIs;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace RangeDisplay.RangeHandling.RangeCreators
{
    internal class SprinklerRangeCreator : IObjectRangeCreator
    {
        public RangeItem HandledRangeItem => RangeItem.Sprinkler;

        private ISimpleSprinklersAPI simpleSprinklersAPI = null;
        private IBetterSprinklersAPI betterSprinklersAPI = null;

        public void ModRegistryReady(IModRegistry registry)
        {
            if (registry.IsLoaded("tZed.SimpleSprinkler"))
            {
                this.simpleSprinklersAPI = registry.GetApi<ISimpleSprinklersAPI>("tZed.SimpleSprinkler");
            }

            if (registry.IsLoaded("Speeder.BetterSprinklers"))
            {
                this.betterSprinklersAPI = registry.GetApi<IBetterSprinklersAPI>("Speeder.BetterSprinklers");
            }
        }

        public bool CanHandle(SObject obj)
        {
            return obj.name.ToLower().Contains("sprinkler");
        }

        public IEnumerable<Vector2> GetRange(SObject obj, Vector2 position, GameLocation location)
        {
            if (this.betterSprinklersAPI == null)
            {
                foreach (Vector2 pos in this.GetDefaultRange(obj, position))
                    yield return pos;
            } else if (this.betterSprinklersAPI.GetSprinklerCoverage().TryGetValue(obj.parentSheetIndex, out Vector2[] bExtra))
                foreach (Vector2 extraPos in bExtra)
                    yield return extraPos + position;

            if (this.simpleSprinklersAPI != null && this.simpleSprinklersAPI.GetNewSprinklerCoverage().TryGetValue(obj.parentSheetIndex, out Vector2[] sExtra))
                foreach(Vector2 extraPos in sExtra)
                    yield return extraPos + position;
        }

        public IEnumerable<Vector2> GetDefaultRange(SObject obj, Vector2 position)
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