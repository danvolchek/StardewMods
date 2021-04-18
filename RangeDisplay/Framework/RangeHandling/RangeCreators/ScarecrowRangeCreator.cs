using Microsoft.Xna.Framework;
using RangeDisplay.Framework.APIs;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace RangeDisplay.Framework.RangeHandling.RangeCreators
{
    /// <summary>Creates range for scarecrows.</summary>
    internal class ScarecrowRangeCreator : IRangeCreator<SObject>, IModRegistryListener
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The handled range item.</summary>
        public RangeItem HandledRangeItem => RangeItem.Scarecrow;

        /*********
        ** Fields
        *********/

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
            return (obj.bigCraftable.Value && obj.name.ToLower().Contains("arecrow")) || (this.prismaticToolsAPI != null && this.prismaticToolsAPI.ArePrismaticSprinklersScarecrows && obj.ParentSheetIndex == this.prismaticToolsAPI.SprinklerIndex);
        }

        /// <summary>Creates a range.</summary>
        /// <param name="obj">The object to create a range for.</param>
        /// <param name="position">The position the object is at.</param>
        /// <param name="location">The location the object is in.</param>
        /// <returns>The created range.</returns>
        /// <remarks>Derived from <see cref="Farm.addCrows"/>.</remarks>
        public IEnumerable<Vector2> CreateRange(SObject obj, Vector2 position, GameLocation location)
        {
            int radius = obj.Name.Contains("Deluxe") ? 17 : 9;

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2 tile = new Vector2(position.X + x, position.Y + y);
                    if (Vector2.Distance(tile, position) < radius)
                        yield return tile;
                }
            }
        }

        /// <summary>Called when the mod registry is ready.</summary>
        /// <param name="registry">The mod registry.</param>
        public void ModRegistryReady(IModRegistry registry)
        {
            if (registry.IsLoaded("stokastic.PrismaticTools"))
            {
                this.prismaticToolsAPI = registry.GetApi<IPrismaticToolsAPI>("stokastic.PrismaticTools");
            }
        }
    }
}
