using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Linq;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace BetterFruitTrees
{
    /***
     * All ye who enter here be warned - this class involves a lot of copy pasting (though not as much as the harvester) due to non-virtual methods and is less than ideal.
     ***/
    class FruitTreeAwareJunimoHut : JunimoHut
    {
        public static IModHelper helper;
        public static bool HarvestThreeAtOnce;

        //Private variables in JunimoHarvester
        private IReflectedField<int> fjunimoSendOutTimer;
        private IReflectedField<int> fnewConstructionTimer;
        private IReflectedField<bool> fwasLit;

        public FruitTreeAwareJunimoHut(BluePrint b, Vector2 tileLocation) : base(b, tileLocation)
        {
            fjunimoSendOutTimer = helper.Reflection.GetField<int>(this, "junimoSendOutTimer");
            fnewConstructionTimer = helper.Reflection.GetField<int>(this, "newConstructionTimer");
            fwasLit = helper.Reflection.GetField<bool>(this, "wasLit");
        }

        //Make the update method create FruitTreeAwareJunimoHarvesters instead of the regular ones.
        //We run into the same base update problem as in the harvester, and our only solution is copy/paste.
        public override void Update(GameTime time)
        {
            //Building update code
            if (this.fnewConstructionTimer.GetValue() > 0)
            {
                this.fnewConstructionTimer.SetValue(this.fnewConstructionTimer.GetValue() - time.ElapsedGameTime.Milliseconds);
                if (this.fnewConstructionTimer.GetValue() <= 0 && this.magical)
                    this.daysOfConstructionLeft = 0;
            }
            this.alpha = Math.Min(1f, this.alpha + 0.05f);
            if (!(!Game1.player.GetBoundingBox().Intersects(new Rectangle(Game1.tileSize * this.tileX, Game1.tileSize * (this.tileY + (-(this.getSourceRectForMenu().Height / 16) + this.tilesHigh)), this.tilesWide * Game1.tileSize, (this.getSourceRectForMenu().Height / 16 - this.tilesHigh) * Game1.tileSize + Game1.tileSize / 2))))
                this.alpha = Math.Max(0.4f, this.alpha - 0.09f);
            //Building update code

            if (this.fjunimoSendOutTimer.GetValue() <= 0)
                return;
            this.fjunimoSendOutTimer.SetValue(this.fjunimoSendOutTimer.GetValue() - time.ElapsedGameTime.Milliseconds);

            if (this.fjunimoSendOutTimer.GetValue() > 0 || this.myJunimos.Count() >= 3 || (Game1.IsWinter || Game1.isRaining) || (!this.areThereMatureCropsWithinRadius() || Game1.farmEvent != null))
                return;
            FruitTreeAwareJunimoHarvester junimoHarvester = new FruitTreeAwareJunimoHarvester(new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)) * (float)Game1.tileSize + new Vector2(0.0f, (float)(Game1.tileSize / 2)), this, this.getUnusedJunimoNumber());
            Game1.getFarm().characters.Add((NPC)junimoHarvester);
            this.myJunimos.Add(junimoHarvester);
            this.fjunimoSendOutTimer.SetValue(1000);
            if (!Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1))), Game1.tileSize, (GameLocation)Game1.getFarm()))
                return;
            try
            {
                Game1.playSound("junimoMeep1");
            }
            catch (Exception ex)
            {
            }
        }

        //Make the ten minute update method call the FruitPokeToHarvest method instead of the regular one.
        public override void performTenMinuteAction(int timeElapsed)
        {
            //building base does nothing

            for (int index = this.myJunimos.Count - 1; index >= 0; --index)
            {
                if (!Game1.getFarm().characters.Contains((NPC)this.myJunimos[index]))
                    this.myJunimos.RemoveAt(index);
                else
                    (this.myJunimos[index] as FruitTreeAwareJunimoHarvester).FruitPokeToHarvest();
            }
            if (this.myJunimos.Count<JunimoHarvester>() < 3 && Game1.timeOfDay < 1900)
                this.fjunimoSendOutTimer.SetValue(1);
            if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400 && (!Game1.IsWinter && Utility.getLightSource(this.tileX + this.tileY * 777) == null) && Game1.random.NextDouble() < 0.2)
            {
                Game1.currentLightSources.Add(new LightSource(4, new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)) * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), 0.5f)
                {
                    identifier = this.tileX + this.tileY * 777
                });
                AmbientLocationSounds.addSound(new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)), 1);
                this.fwasLit.SetValue(true);
            }
            else
            {
                if (Game1.timeOfDay != 2400 || Game1.IsWinter)
                    return;
                Utility.removeLightSource(this.tileX + this.tileY * 777);
                AmbientLocationSounds.removeSound(new Vector2((float)(this.tileX + 1), (float)(this.tileY + 1)));
            }
        }

        //Change the mature crop radius check to include fruit trees.
        //Its alright to leave this as new because it is only called from Update, which is virtual.
        public new bool areThereMatureCropsWithinRadius()
        {
            Farm farm = Game1.getFarm();
            for (int index1 = this.tileX + 1 - 8; index1 < this.tileX + 2 + 8; ++index1)
            {
                for (int index2 = this.tileY - 8 + 1; index2 < this.tileY + 2 + 8; ++index2)
                {
                    Vector2 possiblePos = new Vector2((float)index1, (float)index2);

                    if (farm.isCropAtTile(index1, index2) && (farm.terrainFeatures[possiblePos] as HoeDirt).readyForHarvest())
                    {
                        this.lastKnownCropLocation = new Point(index1, index2);
                        return true;

                    }

                    if (farm.terrainFeatures.ContainsKey(possiblePos) && farm.terrainFeatures[possiblePos] is FruitTree tree && CanTreeBeHarvested(tree))
                    {
                        Point cropLocation = GetUnnocupiedAdjacentLocation(index1, index2, farm);
                        if (cropLocation == Point.Zero)
                            return false;
                        this.lastKnownCropLocation = cropLocation;
                        return true;
                    }

                }
            }

            return false;
        }

        public static bool CanTreeBeHarvested(FruitTree tree)
        {
            return HarvestThreeAtOnce ? (tree.fruitsOnTree == 3) : (tree.fruitsOnTree > 0);
        }

        public static int TreeHarvestAmount()
        {
            return HarvestThreeAtOnce ? 3 : 1;
        }

        /***
         * Gets an unnocupied adjacent tile for a junimo to stand on, prefering the bottom, then right, then left, then top tile.
         ***/
        private Point GetUnnocupiedAdjacentLocation(int tileX, int tileY, GameLocation l)
        {
            if (IsPassableAndUnoccupied(l, tileX, tileY + 1))
                return new Point(tileX, tileY + 1);
            if (IsPassableAndUnoccupied(l, tileX + 1, tileY))
                return new Point(tileX + 1, tileY);
            if (IsPassableAndUnoccupied(l, tileX - 1, tileY))
                return new Point(tileX - 1, tileY);
            if (IsPassableAndUnoccupied(l, tileX, tileY - 1))
                return new Point(tileX, tileY - 1);

            return Point.Zero;
        }

        /***
         * Gets whether the given tile is passable and unnocupied, i.e. could a junimo stand on it.
         ***/
        private bool IsPassableAndUnoccupied(GameLocation location, int x, int y)
        {
            Rectangle tilePixels = new Rectangle((int)(x * Game1.tileSize), (int)(y * Game1.tileSize), Game1.tileSize, Game1.tileSize);
            return IsPassable(location, x, y, tilePixels) && !IsOccupied(location, new Vector2(x, y), tilePixels);
        }

        //Thanks to Pathoschild for these functions (adapted slightly)
        //https://github.com/Pathoschild/StardewMods/blob/data-maps/1.3/DataMaps/DataMaps/AccessibilityMap.cs#L137-L197

        private bool IsPassable(GameLocation location, int x, int y, Rectangle tilePixels)
        {
            // check layer properties
            if (location.isTilePassable(new Location((int)x, (int)y), Game1.viewport))
                return true;

            // allow bridges
            if (location.doesTileHaveProperty((int)x, (int)y, "Passable", "Buildings") != null)
            {
                Tile backTile = location.map.GetLayer("Back").PickTile(new Location(tilePixels.X, tilePixels.Y), Game1.viewport.Size);
                if (backTile == null || !backTile.TileIndexProperties.TryGetValue("Passable", out PropertyValue value) || value != "F")
                    return true;
            }

            return false;
        }

        private bool IsOccupied(GameLocation location, Vector2 tile, Rectangle tilePixels)
        {
            // show open gate as passable
            if (location.objects.TryGetValue(tile, out StardewValley.Object obj) && obj is Fence fence && fence.isGate && fence.gatePosition == Fence.gateOpenedPosition)
                return false;

            // check for objects, characters, or terrain features
            if (location.isTileOccupiedIgnoreFloors(tile))
                return true;

            // buildings
            if (location is BuildableGameLocation buildableLocation)
            {
                foreach (Building building in buildableLocation.buildings)
                {
                    Rectangle buildingArea = new Rectangle(building.tileX, building.tileY, building.tilesWide, building.tilesHigh);
                    if (buildingArea.Contains((int)tile.X, (int)tile.Y))
                        return true;
                }
            }

            // large terrain features
            if (location.largeTerrainFeatures != null && location.largeTerrainFeatures.Any(p => p.getBoundingBox().Intersects(tilePixels)))
                return true;


            return false;
        }
    }
}
