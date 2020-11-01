using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace CactiHurtYou
{
    /// <summary>Makes cacti hurt the player on touch.</summary>
    public class CactiHurtYouMod : Mod
    {
        /// <summary>Known cactus tile indices.</summary>
        private readonly int[] cactusTiles = { 116, 131, 132, 147, 148 };

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            this.Helper.Events.GameLoop.UpdateTicking += this.GameLoop_UpdateTicking;
        }

        /// <summary>Raised before the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation == null || !e.IsMultipleOf(10))
            {
                return;
            }

            Microsoft.Xna.Framework.Rectangle nextPosition = Game1.player.nextPosition(Game1.player.getDirection());

            IEnumerable<Vector2> positionsToCheck = new[]
            {
                new Vector2((int) (nextPosition.Right / 64.0), (int) (nextPosition.Bottom / 64.0)),
                new Vector2((int) (nextPosition.Right / 64.0), (int) (nextPosition.Top / 64.0)),
                new Vector2((int) (nextPosition.Left / 64.0), (int) (nextPosition.Top / 64.0)),
                new Vector2((int) (nextPosition.Left / 64.0), (int) (nextPosition.Bottom / 64.0))
            };

            foreach (Vector2 position in positionsToCheck)
            {
                this.TryHurtBecauseCactus(Game1.currentLocation, position, Game1.player, false);
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation == null)
            {
                return;
            }

            if (e.Button.IsActionButton())
            {
                this.TryHurtBecauseCactus(Game1.currentLocation, e.Cursor.GrabTile, Game1.player, true);
            }
        }

        /// <summary>Tries to hurt the farmer because of cacti.</summary>
        /// <param name="location">The location to look for cacti in.</param>
        /// <param name="tileLocation">Where in the location to look for the cacti.</param>
        /// <param name="who">Which farmer  to hurt.</param>
        /// <param name="isClick">Whether the hurt is caused by a click.</param>
        private void TryHurtBecauseCactus(GameLocation location, Vector2 tileLocation, Farmer who, bool isClick)
        {
            this.TryHurtBecauseCactusTile(location.Map, new Location((int)tileLocation.X, (int)tileLocation.Y), who);
            this.TryHurtBecauseCactusCrop(location, tileLocation, who, isClick);
        }

        /// <summary>Tries to hurt the farmer because of cacti crops.</summary>
        /// <param name="location">The location to look for cacti in.</param>
        /// <param name="tileLocation">Where in the location to look for the cacti.</param>
        /// <param name="who">Which farmer  to hurt.</param>
        /// <param name="isClick">Whether the hurt is caused by a click.</param>
        private void TryHurtBecauseCactusCrop(GameLocation location, Vector2 tileLocation, Farmer who, bool isClick)
        {
            Crop crop = null;
            if (location.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature feature) && feature is HoeDirt dirt && dirt.crop != null)
            {
                crop = dirt.crop;
            }
            else if (location.objects.TryGetValue(tileLocation, out StardewValley.Object obj) && obj is IndoorPot pot && pot.hoeDirt.Value?.crop != null)
            {
                crop = pot.hoeDirt.Value.crop;
            }

            if (crop != null)
            {
                // Hurt when touching cacti out of the seed stage that are not dead, except when clicking to gather fruit.

                bool hasFruit = crop.currentPhase.Value == crop.phaseDays.Count - 1 && crop.dayOfCurrentPhase.Value == 0;

                if (crop.rowInSpriteSheet.Value == 41 && crop.currentPhase.Value != 0 && (!hasFruit || !isClick) && !crop.dead.Value)
                {
                    this.HurtFarmer(who);
                }
            }
        }

        /// <summary>Tries to hurt the farmer because of cacti tiles.</summary>
        /// <param name="map">The map to look for cacti in.</param>
        /// <param name="tileLocation">Where in the location to look for the cacti.</param>
        /// <param name="who">Which farmer  to hurt.</param>
        private void TryHurtBecauseCactusTile(Map map, Location tileLocation, Farmer who)
        {
            if (Game1.currentLocation.Name != "Desert")
            {
                return;
            }

            if (this.IsCactusOnTile(map, tileLocation))
            {
                this.HurtFarmer(who);
            }
        }

        /// <summary>Gets whether a cactus is on the given tile.</summary>
        /// <param name="map">The map to look for cacti in.</param>
        /// <param name="tileLocation">Where in the location to look for the cacti.</param>
        /// <returns>Whether a cactus was found.</returns>
        private bool IsCactusOnTile(Map map, Location tileLocation)
        {
            Layer buildings = map.GetLayer("Buildings");
            if (buildings == null || !buildings.IsValidTileLocation(tileLocation))
            {
                return false;
            }

            Tile tile = buildings.Tiles[tileLocation];

            if (tile != null && this.cactusTiles.Contains(tile.TileIndex))
            {
                return true;
            }

            return false;
        }

        /// <summary>Hurts the farmer a bit. Ouch!</summary>
        /// <param name="who">Which farmer to hurt.</param>
        private void HurtFarmer(Farmer who)
        {
            who.takeDamage(5, true, null);
        }
    }
}
