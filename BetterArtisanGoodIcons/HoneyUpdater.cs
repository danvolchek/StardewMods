using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;

namespace BetterArtisanGoodIcons
{
	internal static class HoneyUpdater
	{
		private const int maxMinutesAwake = 1200;
		private const int flowerRange = 5;
		private const bool shouldOutputDebug = true;

		private static Dictionary<GameLocation, List<SObject>> beeHousesReady = new();
		private static Dictionary<GameLocation, List<SObject>> beeHousesReadyToday = new();
		private static Dictionary<GameLocation, List<Vector2>> closeFlowerTileLocations = new();

		// Shorthand for the debug logger. Also so we can easily disable outputting it.
		private static void DebugLog(string message)
		{
			if (shouldOutputDebug)
			{
				// Show microsecond, too, so we can tell if something is causing performance issues
				ArtisanGoodsManager.Mod.Monitor.Log($"{DateTime.Now:ffffff} {nameof(HoneyUpdater)} {message}", LogLevel.Debug);
			}
		}

		private static void UpdateLocationBeeHouses(GameLocation location, List<SObject> readyBeeHouses)
		{
			DebugLog($"{nameof(UpdateLocationBeeHouses)} - Started");

			List<SObject> invalidBeeHouses = new();

			foreach (SObject beeHouse in readyBeeHouses)
			{
				// If a bee house no longer qualifies, we'll remove it after we go through the list we were given
				if (beeHouse == null || !beeHouse.readyForHarvest.Value || beeHouse.heldObject.Value == null)
				{
					invalidBeeHouses.Add(beeHouse);

					continue;
				}

				// Same flower check the game uses when collecting the honey out of the bee house
				Crop closeFlower = Utility.findCloseFlower(location, beeHouse.TileLocation, flowerRange, (Crop crop) => (!crop.forageCrop.Value) ? true : false);

				// We set the held honey either to the default (in case we are changing it back, such as if all nearby flowers were harvested),
				// or to what the farmer will receive at time of harvest when full-grown flower(s) are nearby enough.
				// The game will overwrite what we've set here, so we won't affect the actually-harvested honey object in any way.
				beeHouse.heldObject.Value.name = closeFlower == null
					? "Wild Honey"
					: $"{Game1.objectInformation[closeFlower.indexOfHarvest.Value].Split('/')[0]} Honey";
				beeHouse.heldObject.Value.preservedParentSheetIndex.Value = closeFlower == null
					? (int)ArtisanGood.Honey
					: closeFlower.indexOfHarvest.Value;

				if (closeFlower != null)
				{
					// Attempt to get the tile location of the `HoeDirt` (inherits from `TerrainFeature`) that holds the flower's `Crop` object, so we can watch for it being harvested later
					KeyValuePair<Vector2, TerrainFeature> closeFlowerTileLocationPair = location.terrainFeatures.Pairs.FirstOrDefault(x => x.Value is HoeDirt && (x.Value as HoeDirt).crop == closeFlower);

					if (!closeFlowerTileLocationPair.Equals(default(KeyValuePair<Vector2, TerrainFeature>))
						&& (!closeFlowerTileLocations.ContainsKey(location) || !closeFlowerTileLocations[location].Contains(closeFlowerTileLocationPair.Key)))
					{
						if (!closeFlowerTileLocations.ContainsKey(location))
						{
							closeFlowerTileLocations.Add(location, new List<Vector2>());
						}

						closeFlowerTileLocations[location].Add(closeFlowerTileLocationPair.Key);

						DebugLog($"Found tile {closeFlowerTileLocationPair.Key} matching the nearby grown flower affecting bee house @ {beeHouse.TileLocation} tile @ {location.Name} location");
					}
				}

				DebugLog($"Assigned {beeHouse.heldObject.Value.name} to bee house @ {beeHouse.TileLocation} tile @ {location.Name} location");
			}

			// Remove any invalid bee houses from the given list
			readyBeeHouses.RemoveAll(x => invalidBeeHouses.Contains(x));

			DebugLog($"{nameof(UpdateLocationBeeHouses)} - Ended");
		}

		private static void AddLocation(GameLocation location)
		{
			List<SObject> ready = location.Objects.Values.Where(x => x.bigCraftable.Value && x.name.Equals("Bee House")
				&& x.readyForHarvest.Value && x.heldObject.Value != null).ToList();
			
			List<SObject> readyToday = location.Objects.Values.Where(x => x.bigCraftable.Value && x.name.Equals("Bee House")
				&& !x.readyForHarvest.Value && x.heldObject.Value != null && x.MinutesUntilReady <= maxMinutesAwake).ToList();

			if (ready.Count > 0)
			{
				beeHousesReady.Add(location, ready);
				UpdateLocationBeeHouses(location, ready);

				DebugLog($"{nameof(AddLocation)} - Found and updated {ready.Count} ready bee houses "
					+ (closeFlowerTileLocations.ContainsKey(location) ? $"and {closeFlowerTileLocations[location].Count} close flowers" : String.Empty)
					+ $" @ {location.Name} location");
			}

			if (readyToday.Count > 0)
			{
				beeHousesReadyToday.Add(location, readyToday);

				DebugLog($"{nameof(AddLocation)} - Found {ready.Count} bee houses that will be ready today @ {location.Name} location");
			}
		}

		/// <summary>Event handler for after a new day starts.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		internal static void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			DebugLog($"{nameof(OnDayStarted)} - Started");

			// Reset our tracked bee houses and flowers for the new day
			beeHousesReady = new();
			beeHousesReadyToday = new();
			closeFlowerTileLocations = new();

			// Filter to just locations we care about.
			// Note that both `OnLocationListChanged` and `OnObjectListChanged` both use filtering based on this,
			// so update those (and probably some other logic in this class), too, if this needs to change.
			foreach (GameLocation location in Game1.locations.Where(x => x.IsOutdoors && x.Objects.Values.Any(y => y.bigCraftable.Value && y.name.Equals("Bee House"))))
			{
				AddLocation(location);
			}

			DebugLog($"{nameof(OnDayStarted)} - Ended");
		}

		/// <summary>Event handler for when the in-game clock changes.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		internal static void OnTimeChanged(object sender, TimeChangedEventArgs e)
		{
			foreach (KeyValuePair<GameLocation, List<SObject>> entry in beeHousesReadyToday)
			{
				List<SObject> newlyReadyBeeHouses = entry.Value.Where(x => x.readyForHarvest.Value).ToList();

				if (newlyReadyBeeHouses.Count == 0)
				{
					DebugLog($"{nameof(OnTimeChanged)} - Ended");

					return;
				}

				DebugLog($"{nameof(OnTimeChanged)} - Found {newlyReadyBeeHouses.Count} newly ready bee houses @ {entry.Key.Name} location");

				UpdateLocationBeeHouses(entry.Key, newlyReadyBeeHouses);

				if (!beeHousesReady.ContainsKey(entry.Key))
				{
					beeHousesReady.Add(entry.Key, new List<SObject>());
				}

				beeHousesReady[entry.Key].AddRange(newlyReadyBeeHouses);
				beeHousesReadyToday[entry.Key].RemoveAll(x => newlyReadyBeeHouses.Contains(x));
			}
		}

		/// <summary>Event handler for after the game state is updated, once per second.</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		internal static void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
		{
			// Check if flowers that would affect the honey nearby bee houses show have been harvested
			foreach (KeyValuePair<GameLocation, List<Vector2>> entry in closeFlowerTileLocations)
			{
				// Find any tiles we were tracking at this location that no longer have a crop (flower) attached to them
				List<Vector2> harvestedFlowerLocations = entry.Key.terrainFeatures.Pairs
					.Where(tfp => entry.Value.Contains(tfp.Key) && tfp.Value is HoeDirt && (tfp.Value as HoeDirt).crop == null)
					.Select(x => x.Key)
					.ToList();

				if (harvestedFlowerLocations.Count == 0)
				{
					continue;
				}

				DebugLog($"{nameof(OnOneSecondUpdateTicked)} - Found {harvestedFlowerLocations.Count} harvested flowers @ {entry.Key.Name} location.\n"
					+ $"    Flower coords: {String.Join(" | ", harvestedFlowerLocations)}");

				// Remove the flower tile(s) from being tracked
				closeFlowerTileLocations[entry.Key].RemoveAll(x => harvestedFlowerLocations.Contains(x));

				List<SObject> beeHousesToUpdate = new();

				foreach (Vector2 tileLocation in harvestedFlowerLocations)
				{
					// Update any bee house within the effective range radius of the removed flower.
					beeHousesToUpdate.AddRange(beeHousesReady[entry.Key].Where(beeHouse =>
						beeHouse.TileLocation.X <= tileLocation.X + flowerRange && beeHouse.TileLocation.X >= Math.Max(tileLocation.X - flowerRange, 0)
						&& beeHouse.TileLocation.Y <= tileLocation.Y + flowerRange && beeHouse.TileLocation.Y >= Math.Max(tileLocation.Y - flowerRange, 0)
						&& !beeHousesToUpdate.Contains(beeHouse)
					));
				}

				if (beeHousesToUpdate.Count == 0)
				{
					continue;
				}

				UpdateLocationBeeHouses(entry.Key, beeHousesToUpdate);

				DebugLog($"{nameof(OnOneSecondUpdateTicked)} - Found {beeHousesToUpdate.Count} ready bee houses that need updating @ {entry.Key.Name} location.\n"
					+ $"    Bee house coords: {String.Join(" | ", beeHousesToUpdate.Select(x => x.TileLocation))}");
			}
		}

		/// <summary>
		/// Event handler for after objects are added/removed in any location (including machines, fences, etc).
		/// This doesn't apply for floating items (see DebrisListChanged) or furniture (see FurnitureListChanged).
		/// This event isn't raised for objects already present when a location is added. If you need to handle those too, use `LocationListChanged` and check `e.Added → objects`.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		internal static void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
		{
			// Check the location and objects similar to how we location-filter in `OnDayStarted`
			if (!e.Removed.Any()
				|| !e.Location.IsOutdoors
				|| (!beeHousesReady.ContainsKey(e.Location) && !beeHousesReadyToday.ContainsKey(e.Location))
				|| !e.Removed.Any(x => x.Value.bigCraftable.Value && x.Value.name.Equals("Bee House") && x.Value.heldObject.Value != null))
			{
				return;
			}

			// Find all removed bee houses so we can remove them from our tracking dictionaries
			IEnumerable<SObject> removedBeeHouses = e.Removed.Select(y => y.Value).Where(z => z.bigCraftable.Value && z.name.Equals("Bee House") && z.heldObject.Value != null);
			DebugLog($"{nameof(OnObjectListChanged)} - Found {removedBeeHouses.Count()} bee houses to attempt to remove from tracking");

			if (beeHousesReady.ContainsKey(e.Location) && beeHousesReady[e.Location].Any(x => removedBeeHouses.Contains(x)))
			{
				beeHousesReady[e.Location].RemoveAll(x => removedBeeHouses.Contains(x));
				DebugLog($"{nameof(OnObjectListChanged)} - {e.Location} location has {beeHousesReady[e.Location].Count} remaining tracked ready bee houses");
			}

			if (beeHousesReadyToday.ContainsKey(e.Location) && beeHousesReadyToday[e.Location].Any(x => removedBeeHouses.Contains(x)))
			{
				beeHousesReadyToday[e.Location].RemoveAll(x => removedBeeHouses.Contains(x));
				DebugLog($"{nameof(OnObjectListChanged)} - {e.Location} location has {beeHousesReadyToday[e.Location].Count} remaining tracked ready-today bee houses");
			}
		}

		/// <summary>Event handler for after a game location is added or removed (including building interiors).</summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		internal static void OnLocationListChanged(object sender, LocationListChangedEventArgs e)
		{
			// Use the same location-filtering we do up in `OnDayStarted`
			foreach (GameLocation addedLocation in e.Added.Where(x => x.IsOutdoors && x.Objects.Values.Any(y => y.bigCraftable.Value && y.name.Equals("Bee House"))))
			{
				// If we somehow have the location as a key already, reset their lists before we (re-)add the location
				if (beeHousesReady.ContainsKey(addedLocation))
				{
					beeHousesReady[addedLocation].Clear();
				}

				if (beeHousesReadyToday.ContainsKey(addedLocation))
				{
					beeHousesReadyToday[addedLocation].Clear();
				}

				if (closeFlowerTileLocations.ContainsKey(addedLocation))
				{
					closeFlowerTileLocations[addedLocation].Clear();
				}

				AddLocation(addedLocation);
			}

			// Clear any data we are tracking about this location
			foreach (GameLocation removedLocation in e.Removed.Where(x => beeHousesReady.ContainsKey(x) || beeHousesReadyToday.ContainsKey(x)))
			{
				if (beeHousesReady.ContainsKey(removedLocation))
				{
					beeHousesReady.Remove(removedLocation);
				}

				if (beeHousesReadyToday.ContainsKey(removedLocation))
				{
					beeHousesReadyToday.Remove(removedLocation);
				}

				if (closeFlowerTileLocations.ContainsKey(removedLocation))
				{
					closeFlowerTileLocations.Remove(removedLocation);
				}
			}
		}
	}
}
