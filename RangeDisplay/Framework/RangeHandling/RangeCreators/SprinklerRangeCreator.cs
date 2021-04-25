using Microsoft.Xna.Framework;
using RangeDisplay.Framework.APIs;
using StardewModdingAPI;
using StardewValley;
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
                    foreach (Vector2 pos in obj.GetSprinklerTiles())
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
