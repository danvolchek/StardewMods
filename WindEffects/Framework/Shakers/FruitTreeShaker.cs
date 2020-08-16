using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace WindEffects.Framework.Shakers
{
    class FruitTreeShaker : IShaker
    {
        private readonly FruitTree fruitTree;
        private readonly bool left;

        public FruitTreeShaker(FruitTree fruitTree, bool left)
        {
            this.fruitTree = fruitTree;
            this.left = left;
        }

        public void Shake(IReflectionHelper helper, Vector2 tile)
        {
            // can't just call shake because it drops items. We don't want to drop items.
            // unfortunately almost identical, but different from, TreeShaker
            // see FruitTree::shake for the logic this replicates

            // already shaking
            if (helper.GetField<float>(fruitTree, "maxShake").GetValue() != 0)
                return;

            fruitTree.shakeLeft.Value = this.left;
            helper.GetField<float>(fruitTree, "maxShake").SetValue((float)(fruitTree.growthStage.Value >= 5 ? Math.PI / 128f : Math.PI / 64f));

            if (fruitTree.growthStage.Value >= 3 && !fruitTree.stump.Value)
            {
                if (fruitTree.growthStage.Value >= 4)
                {
                    if (Game1.random.NextDouble() < 0.66)
                    {
                        List<Leaf> leaves = helper.GetField<List<Leaf>>(this.fruitTree, "leaves").GetValue();

                        int num = Game1.random.Next(1, 6);
                        for (int index = 0; index < num; ++index)
                            leaves.Add(new Leaf(new Vector2((float)Game1.random.Next((int)((double)tile.X * 64.0 - 64.0), (int)((double)tile.X * 64.0 + 128.0)), (float)Game1.random.Next((int)((double)tile.Y * 64.0 - 256.0), (int)((double)tile.Y * 64.0 - 192.0))), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(5) / 10f));
                    }
                }
                else
                {
                    if (Game1.random.NextDouble() < 0.66)
                    {
                        List<Leaf> leaves = helper.GetField<List<Leaf>>(this.fruitTree, "leaves").GetValue();

                        int num = Game1.random.Next(1, 3);
                        for (int index = 0; index < num; ++index)
                            leaves.Add(new Leaf(new Vector2((float)Game1.random.Next((int)((double)tile.X * 64.0), (int)((double)tile.X * 64.0 + 48.0)), (float)((double)tile.Y * 64.0 - 32.0)), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(30) / 10f));
                    }
                }
            }
            else
            {
                if (fruitTree.stump.Value)
                    helper.GetField<float>(fruitTree, "shakeTimer").SetValue(100);
            }
        }
    }
}
