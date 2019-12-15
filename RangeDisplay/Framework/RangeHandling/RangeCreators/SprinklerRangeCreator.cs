using Microsoft.Xna.Framework;
using RangeDisplay.Framework.APIs;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace RangeDisplay.Framework.RangeHandling.RangeCreators
{
    /// <summary>Creates range for sprinklers.</summary>
    internal class SprinklerRangeCreator : IRangeCreator<SObject>, IModRegistryListener
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The handled range item.</summary>
        public RangeItem HandledRangeItem => RangeItem.Sprinkler;

        /*********
        ** Fields
        *********/

        /// <summary>The simple sprinklers API.</summary>
        private ISimpleSprinklersAPI simpleSprinklersAPI;

        /// <summary>The better sprinklers API.</summary>
        private IBetterSprinklersAPI betterSprinklersAPI;

        /// <summary>The prismatic tools API.</summary>
        private IPrismaticToolsAPI prismaticToolsAPI;

        /*********
        ** Public methods
        *********/

        /// <summary>Whether this creator can create range for the given object.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>Whether range can be created for it.</returns>
        public bool CanCreateRangeFor(SObject obj)
        {
            return obj.name.ToLower().Contains("sprinkler");
        }

        /// <summary>Creates a range.</summary>
        /// <param name="obj">The object to create a range for.</param>
        /// <param name="position">The position the object is at.</param>
        /// <param name="location">The location the object is in.</param>
        /// <returns>The created range.</returns>
        public IEnumerable<Vector2> CreateRange(SObject obj, Vector2 position, GameLocation location)
        {
            if (this.prismaticToolsAPI != null && obj.ParentSheetIndex == this.prismaticToolsAPI.SprinklerIndex)
                foreach (Vector2 pos in this.prismaticToolsAPI.GetSprinklerCoverage(position))
                    yield return pos;
            else
            {
                if (this.betterSprinklersAPI == null)
                {
                    foreach (Vector2 pos in this.CreateVanillaRange(obj, position))
                    {
                        yield return pos;
                    }
                }
                else if (this.betterSprinklersAPI.GetSprinklerCoverage().TryGetValue(obj.ParentSheetIndex, out Vector2[] bExtra))
                {
                    foreach (Vector2 extraPos in bExtra)
                    {
                        yield return extraPos + position;
                    }
                }

                if (this.simpleSprinklersAPI != null && this.simpleSprinklersAPI.GetNewSprinklerCoverage().TryGetValue(obj.ParentSheetIndex, out Vector2[] sExtra))
                {
                    foreach (Vector2 extraPos in sExtra)
                    {
                        yield return extraPos + position;
                    }
                }
            }
        }

        /// <summary>Creates the vanilla range for sprinklers.</summary>
        /// <param name="obj">The sprinkler.</param>
        /// <param name="position">The position the sprinkler is at.</param>
        /// <returns>The range.</returns>
        public IEnumerable<Vector2> CreateVanillaRange(SObject obj, Vector2 position)
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

        /// <summary>Called when the mod registry is ready.</summary>
        /// <param name="registry">The mod registry.</param>
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

            if (registry.IsLoaded("stokastic.PrismaticTools"))
            {
                this.prismaticToolsAPI = registry.GetApi<IPrismaticToolsAPI>("stokastic.PrismaticTools");
            }
        }
    }
}
