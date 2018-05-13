using System.Collections.Generic;
using StardewValley;

namespace CustomizableDeathPenalty
{
    /***
     * Represents all the fields that can change due to death.
     ***/
    internal class PlayerState
    {
        public int deepestMineLevel;
        public IList<Item> inventory;
        public int lowestLevelReached;
        public int money;

        public PlayerState(int m, int d, int l, IEnumerable<Item> i)
        {
            this.money = m;
            this.deepestMineLevel = d;
            this.lowestLevelReached = l;
            this.inventory = new List<Item>();
            foreach (var item in i)
                if (item == null)
                {
                    this.inventory.Add(null);
                }
                else
                {
                    var copy = item.getOne();
                    copy.Stack = item.Stack;

                    this.inventory.Add(copy);
                }
        }
    }
}