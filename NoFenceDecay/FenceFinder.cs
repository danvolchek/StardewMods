using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

namespace NoFenceDecay
{
    /// <summary>Finds all fences in the game.</summary>
    internal class FenceFinder
    {
        /*********
        ** Fields
        *********/

        /// <summary>Avoid infinite recursion for mods that mess up <see cref="StardewValley.Buildings.Building.indoors" />.</summary>
        private readonly IList<GameLocation> searchedLocations = new List<GameLocation>();

        /*********
        ** Public methods
        *********/

        /// <summary>Finds all fences in the game.</summary>
        public IEnumerable<Fence> GetFences()
        {
            this.searchedLocations.Clear();
            return !Context.IsWorldReady ? new Fence[0] : this.GetFences(Game1.locations);
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Finds all fences in the given <see cref="GameLocation" />s.</summary>
        private IEnumerable<Fence> GetFences(IEnumerable<GameLocation> locations)
        {
            return locations.SelectMany(this.GetFences);
        }

        /// <summary>Finds all fences in the given <see cref="GameLocation" />.</summary>
        private IEnumerable<Fence> GetFences(GameLocation l)
        {
            if (l == null || this.searchedLocations.Contains(l))
            {
                yield break;
            }

            this.searchedLocations.Add(l);
            foreach (Fence f in l.Objects.Values.OfType<Fence>())
            {
                yield return f;
            }

            if (!(l is BuildableGameLocation bLoc))
            {
                yield break;
            }

            foreach (Fence f in this.GetFences(bLoc.buildings.Where(item => item != null).Select(item => item.indoors.Value).Where(item => item != null)))
            {
                yield return f;
            }
        }
    }
}
