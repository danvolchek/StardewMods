using System;
using System.Collections.Generic;
using System.Linq;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;
using StardewValley.Menus;

using CustomElementHandler;

namespace GiantCropRing
{
    public class ModEntry : Mod
    {
        private ModConfig config;
        private Texture2D giantRingTexture;

        public override void Entry(IModHelper helper)
        {
            SaveEvents.BeforeSave += this.BeforeSave;
            MenuEvents.MenuChanged += this.MenuChanged;
            config = helper.ReadConfig<ModConfig>();
            giantRingTexture = Helper.Content.Load<Texture2D>("assets/ring.png");

            GiantRing.texture = giantRingTexture;
            GiantRing.price = config.cropRingPrice / 2;


        }

        private void MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {

            if (Game1.activeClickableMenu is ShopMenu)
            {

                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                if (shop.portraitPerson is NPC && shop.portraitPerson.name == "Pierre")// && Game1.dayOfMonth % 7 == )
                {

                    Dictionary<Item, int[]> items = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(shop, "itemPriceAndStock");
                    List<Item> selling = Helper.Reflection.GetPrivateValue<List<Item>>(shop, "forSale");

                    GiantRing ring = new GiantRing();
                    items.Add(ring, new int[] { config.cropRingPrice, int.MaxValue });
                    selling.Add(ring);

                }
            }
        }

        private void BeforeSave(object sender, EventArgs e)
        {
            double chance = 0.0;
            if (Game1.player.leftRing is GiantRing)
            {
                chance += config.cropChancePercentWithRing;
            }
            if (Game1.player.rightRing is GiantRing)
            {
                chance += config.cropChancePercentWithRing;
            }

            if (!config.shouldWearingBothRingsDoublePercentage && chance > config.cropChancePercentWithRing)
                chance = config.cropChancePercentWithRing;


            if (chance > 0)
                maybeChangeCrops(chance, Game1.getFarm());
        }

        private void maybeChangeCrops(double chance, GameLocation environment)
        {

            foreach (Tuple<Vector2, Crop> tup in getValidCrops())
            {
                int xTile = (int)tup.Item1.X;
                int yTile = (int)tup.Item1.Y;

                Crop crop = tup.Item2;


                double rand = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + xTile * 2000 + yTile).NextDouble();


                bool okCrop = true;
                if (crop.currentPhase == crop.phaseDays.Count - 1 && (crop.indexOfHarvest == 276 || crop.indexOfHarvest == 190 || crop.indexOfHarvest == 254) && rand < chance)
                {

                    for (int index1 = xTile - 1; index1 <= xTile + 1; ++index1)
                    {
                        for (int index2 = yTile - 1; index2 <= yTile + 1; ++index2)
                        {

                            Vector2 key = new Vector2((float)index1, (float)index2);
                            if (!environment.terrainFeatures.ContainsKey(key) || !(environment.terrainFeatures[key] is HoeDirt) || ((environment.terrainFeatures[key] as HoeDirt).crop == null || (environment.terrainFeatures[key] as HoeDirt).crop.indexOfHarvest != crop.indexOfHarvest))
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
                    {
                        for (int index2 = yTile - 1; index2 <= yTile + 1; ++index2)
                        {
                            Vector2 index3 = new Vector2((float)index1, (float)index2);
                            (environment.terrainFeatures[index3] as HoeDirt).crop = (Crop)null;
                        }
                    }

                    (environment as Farm).resourceClumps.Add(new GiantCrop(crop.indexOfHarvest, new Vector2((float)(xTile - 1), (float)(yTile - 1))));

                }

            }

        }

        private List<Tuple<Vector2, Crop>> getValidCrops()
        {

            return Game1.locations.Where(gl => gl is Farm).SelectMany(gl => (gl as Farm).terrainFeatures.Where(tf => tf.Value is HoeDirt && (tf.Value as HoeDirt).crop != null
            && (tf.Value as HoeDirt).state == 1).Select(hd => new Tuple<Vector2, Crop>(hd.Key, (hd.Value as HoeDirt).crop))
            .Where(c => !(c.Item2.dead || !c.Item2.seasonsToGrowIn.Contains(Game1.currentSeason)))).ToList();
        }
    }
}
