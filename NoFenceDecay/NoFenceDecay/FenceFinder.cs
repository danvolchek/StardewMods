using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace NoFenceDecay
{
    /// <summary>Finds all fences in the game.</summary>
    internal class FenceFinder
    {
        /// <summary>Avoid infinite recursion for mods that mess up <see cref="StardewValley.Buildings.Building.indoors" />.</summary>
        private readonly IList<GameLocation> searchedLocations = new List<GameLocation>();

        /// <summary>Finds all fences in the game.</summary>
        internal IEnumerable<Fence> GetFences()
        {
            this.searchedLocations.Clear();
            if (Context.IsWorldReady)
                foreach (Fence f in this.GetFences(Game1.locations))
                    yield return f;
        }

        /// <summary>Finds all fences in the given <see cref="GameLocation" />s.</summary>
        private IEnumerable<Fence> GetFences(IEnumerable<GameLocation> locations)
        {
            foreach (Fence f in locations.SelectMany(this.GetFences))
                yield return f;
        }

        /// <summary>Finds all fences in the given <see cref="GameLocation" />.</summary>
        private IEnumerable<Fence> GetFences(GameLocation l)
        {
            if (l == null || this.searchedLocations.Contains(l)) yield break;

            this.searchedLocations.Add(l);
            foreach (Fence f in l.Objects.Values.OfType<Fence>())
                yield return f;

            if (!(l is BuildableGameLocation bLoc)) yield break;

            foreach (Fence f in this.GetFences(bLoc.buildings.Where(item => item != null)
                .Select(item => item.indoors.Value)
                .Where(item => item != null)))
                yield return f;
        }
    }
}