using StardewValley;
using System.Collections.Generic;

namespace CustomizableDeathPenalty
{
    /***
     * Represents all the fields that can change due to death.
     ***/
    class PlayerState
    {
        public int money;
        public int deepestMineLevel;
        public int lowestLevelReached;
        public List<Item> inventory;

        public PlayerState(int m, int d, int l, List<Item> i)
        {
            money = m;
            deepestMineLevel = d;
            lowestLevelReached = l;
            inventory = new List<Item>();
            foreach (Item item in i)
            {
                if (item == null)
                {
                    inventory.Add(null);
                }
                else
                {
                    Item copy = item.getOne();
                    copy.Stack = item.Stack;

                    inventory.Add(copy);
                }
            }
        }
    }
}
