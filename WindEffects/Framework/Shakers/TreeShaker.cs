using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace WindEffects.Framework.Shakers
{
    internal class TreeShaker : IShaker
    {
        private readonly Tree tree;
        private readonly GameLocation location;
        private readonly bool left;

        public TreeShaker(Tree tree,  GameLocation location, bool left)
        {
            this.tree = tree;
            this.location = location;
            this.left = left;
        }

        public void Shake(IReflectionHelper helper, Vector2 tile)
        {
            // can't just call shake because it drops items. We don't want to drop items.
            // see Tree::shake for the logic this replicates

            // deactivated via config
	    if (!ModEntry.config.ShakeTrees)
		return;
            
            // not outdoors
            if (!Game1.player.currentLocation.IsOutdoors)
                return;

            // already shaking
            if (helper.GetField<float>(tree, "maxShake").GetValue() != 0)
                return;

            tree.shakeLeft.Value = this.left;
            helper.GetField<float>(tree, "maxShake").SetValue((float)(tree.growthStage.Value >= 5 ? Math.PI / 128f : Math.PI / 64f));

            if (tree.growthStage.Value >= 3 && !tree.stump.Value)
            {
                if (tree.growthStage.Value >= 5)
                {
                    if (Game1.random.NextDouble() < 0.66)
                    {
                        List<Leaf> leaves = helper.GetField<List<Leaf>>(this.tree, "leaves").GetValue();

                        int num = Game1.random.Next(1, 6);
                        for (int index = 0; index < num; ++index)
                            leaves.Add(new Leaf(new Vector2((float)Game1.random.Next((int)((double)tile.X * 64.0 - 64.0), (int)((double)tile.X * 64.0 + 128.0)), (float)Game1.random.Next((int)((double)tile.Y * 64.0 - 256.0), (int)((double)tile.Y * 64.0 - 192.0))), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(5) / 10f));
                    }

                    if (Game1.random.NextDouble() < 0.01 && (this.tree.currentLocation.GetSeasonForLocation().Equals("spring") || this.tree.currentLocation.GetSeasonForLocation().Equals("summer") || this.tree.currentLocation.GetLocationContext() == GameLocation.LocationContext.Island))
                    {
                        while (Game1.random.NextDouble() < 0.8)
                            location.addCritter((Critter)new Butterfly(new Vector2(tile.X + (float)Game1.random.Next(1, 3), tile.Y - 2f + (float)Game1.random.Next(-1, 2)), this.tree.currentLocation.GetLocationContext() == GameLocation.LocationContext.Island));
                    }
                }
                else
                {
                    if (Game1.random.NextDouble() < 0.66)
                    {
                        List<Leaf> leaves = helper.GetField<List<Leaf>>(this.tree, "leaves").GetValue();

                        int num = Game1.random.Next(1, 3);
                        for (int index = 0; index < num; ++index)
                            leaves.Add(new Leaf(new Vector2((float)Game1.random.Next((int)((double)tile.X * 64.0), (int)((double)tile.X * 64.0 + 48.0)), (float)((double)tile.Y * 64.0 - 32.0)), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(30) / 10f));
                    }
                }
            }
            else
            {
                if (tree.stump.Value)
                    helper.GetField<float>(tree, "shakeTimer").SetValue(100);
            }
        }
    }
}
