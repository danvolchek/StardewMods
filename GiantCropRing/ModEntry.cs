using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GiantCropRing
{
    public class ModEntry : Mod
    {
        private ModConfig config;
        private Texture2D giantRingTexture;

        private int numberOfTimeTicksWearingOneRing;
        private int numberOfTimeTicksWearingTwoRings;

        private int totalNumberOfSeenTimeTicks;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            this.config = helper.ReadConfig<ModConfig>();
            this.giantRingTexture = this.Helper.Content.Load<Texture2D>("assets/ring.png");

            GiantRing.texture = this.giantRingTexture;
            GiantRing.price = this.config.cropRingPrice / 2;
        }

        /// <summary>Raised after the in-game clock time changes.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            bool left = Game1.player.leftRing.Value is GiantRing;
            bool right = Game1.player.rightRing.Value is GiantRing;

            if (left && right)
            {
                this.numberOfTimeTicksWearingOneRing++;
                this.numberOfTimeTicksWearingTwoRings++;
            }
            else if (left || right)
                this.numberOfTimeTicksWearingOneRing++;

            this.totalNumberOfSeenTimeTicks++;
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                if (shop.portraitPerson != null && shop.portraitPerson.Name == "Pierre") // && Game1.dayOfMonth % 7 == )
                {
                    var ring = new GiantRing();

                    shop.itemPriceAndStock.Add(ring, new []{this.config.cropRingPrice, int.MaxValue} );
                    shop.forSale.Add(ring);
                }
            }
        }

        /// <summary>Raised before the game ends the current day. This happens before it starts setting up the next day and before <see cref="IGameLoopEvents.Saving"/>.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            double chance = 0.0;

            this.totalNumberOfSeenTimeTicks = Math.Max(1, this.totalNumberOfSeenTimeTicks);
            this.numberOfTimeTicksWearingOneRing = Math.Max(1, this.numberOfTimeTicksWearingOneRing);
            this.numberOfTimeTicksWearingTwoRings = Math.Max(1, this.numberOfTimeTicksWearingTwoRings);

            if (this.numberOfTimeTicksWearingOneRing / (this.totalNumberOfSeenTimeTicks * 1.0) >= this.config.percentOfDayNeededToWearRingToTriggerEffect)
                chance = this.config.cropChancePercentWithRing;

            if (this.config.shouldWearingBothRingsDoublePercentage && this.numberOfTimeTicksWearingTwoRings / (this.totalNumberOfSeenTimeTicks * 1.0) >= this.config.percentOfDayNeededToWearRingToTriggerEffect)
                chance = 2 * this.config.cropChancePercentWithRing;

            if (chance > 0) this.MaybeChangeCrops(chance, Game1.getFarm());

            this.numberOfTimeTicksWearingOneRing = 0;
            this.numberOfTimeTicksWearingTwoRings = 0;
            this.totalNumberOfSeenTimeTicks = 0;
        }

        private void MaybeChangeCrops(double chance, GameLocation environment)
        {
            foreach (Tuple<Vector2, Crop> tup in this.GetValidCrops())
            {
                int xTile = (int)tup.Item1.X;
                int yTile = (int)tup.Item1.Y;

                Crop crop = tup.Item2;

                double rand = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + xTile * 2000 +
                                      yTile).NextDouble();

                bool okCrop = true;
                if (crop.currentPhase.Value == crop.phaseDays.Count - 1 &&
                    (crop.indexOfHarvest.Value == 276 || crop.indexOfHarvest.Value == 190 || crop.indexOfHarvest.Value == 254) &&
                    rand < chance)
                {
                    for (int index1 = xTile - 1; index1 <= xTile + 1; ++index1)
                    {
                        for (int index2 = yTile - 1; index2 <= yTile + 1; ++index2)
                        {
                            Vector2 key = new Vector2(index1, index2);
                            if (!environment.terrainFeatures.ContainsKey(key) ||
                                !(environment.terrainFeatures[key] is HoeDirt) ||
                                (environment.terrainFeatures[key] as HoeDirt).crop == null ||
                                (environment.terrainFeatures[key] as HoeDirt).crop.indexOfHarvest !=
                                crop.indexOfHarvest)
                            {
                                okCrop = false;

                                break;
                            }
                        }

                        if (!okCrop)
                            break;
                    }

                    if (!okCrop)
                        continue;

                    for (int index1 = xTile - 1; index1 <= xTile + 1; ++index1)
                        for (int index2 = yTile - 1; index2 <= yTile + 1; ++index2)
                        {
                            var index3 = new Vector2(index1, index2);
                            (environment.terrainFeatures[index3] as HoeDirt).crop = null;
                        }

                    (environment as Farm).resourceClumps.Add(new GiantCrop(crop.indexOfHarvest.Value,
                        new Vector2(xTile - 1, yTile - 1)));
                }
            }
        }

        private List<Tuple<Vector2, Crop>> GetValidCrops()
        {
            return Game1.locations.Where(gl => gl is Farm).SelectMany(gl => (gl as Farm).terrainFeatures.Pairs.Where(
                    tf =>
                        tf.Value is HoeDirt hd && hd.crop != null
                                               && hd.state.Value == 1).Select(hd =>
                    new Tuple<Vector2, Crop>(hd.Key, (hd.Value as HoeDirt).crop))
                .Where(c => !(c.Item2.dead.Value || !c.Item2.seasonsToGrowIn.Contains(Game1.currentSeason)))).ToList();
        }
    }
}
