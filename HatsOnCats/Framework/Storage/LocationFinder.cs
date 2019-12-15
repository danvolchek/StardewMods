using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

namespace HatsOnCats.Framework.Storage
{
    /// <summary>Finds all locations in the game.</summary>
    internal class LocationFinder
    {
        /*********
        ** Fields
        *********/

        /// <summary>Avoid infinite recursion for mods that mess up <see cref="StardewValley.Buildings.Building.indoors" />.</summary>
        private readonly IList<GameLocation> searchedLocations = new List<GameLocation>();

        /*********
        ** Public methods
        *********/

        /// <summary>Finds all locations in the game.</summary>
        public IEnumerable<GameLocation> GetLocations()
        {
            this.searchedLocations.Clear();
            return !Context.IsWorldReady ? new GameLocation[0] : this.GetLocations(Game1.locations);
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Finds all locations in the given <see cref="GameLocation" />s.</summary>
        private IEnumerable<GameLocation> GetLocations(IEnumerable<GameLocation> locations)
        {
            return locations.SelectMany(this.GetLocations);
        }

        /// <summary>Finds all locations in the given <see cref="GameLocation" />.</summary>
        private IEnumerable<GameLocation> GetLocations(GameLocation search)
        {
            if (search == null || this.searchedLocations.Contains(search))
            {
                yield break;
            }

            this.searchedLocations.Add(search);
            yield return search;

            if (!(search is BuildableGameLocation bLoc))
            {
                yield break;
            }

            foreach (GameLocation indoors in this.GetLocations(bLoc.buildings.Select(item => item.indoors.Value)))
            {
                yield return indoors;
            }
        }
    }
}
