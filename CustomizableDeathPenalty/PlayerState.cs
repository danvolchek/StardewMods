using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

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
            foreach (Item item in i)
                if (item == null)
                {
                    this.inventory.Add(null);
                }
                else
                {
                    Item copy;
                    if (item is Slingshot slingshot)
                    {
                        Slingshot slingshotCopy = slingshot.getOne() as Slingshot;
                        slingshotCopy.InitialParentTileIndex = slingshot.InitialParentTileIndex;
                        slingshotCopy.CurrentParentTileIndex = slingshot.InitialParentTileIndex;
                        slingshotCopy.IndexOfMenuItemView = slingshot.IndexOfMenuItemView;

                        if (slingshot.attachments[0] != null)
                        {
                            slingshotCopy.attachments[0] = slingshot.attachments[0].getOne() as StardewValley.Object;
                            slingshotCopy.attachments[0].Stack = slingshot.attachments[0].Stack;
                        }
                        copy = slingshotCopy;
                    }else if (item is FishingRod rod)
                    {
                        FishingRod rodCopy = rod.getOne() as FishingRod;

                        for (int index = 0; index < rodCopy.numAttachmentSlots.Value; index++)
                        {
                            if (rod.attachments[index] != null)
                            {
                                rodCopy.attachments[index] = rod.attachments[index].getOne() as StardewValley.Object;
                                rodCopy.attachments[index].Stack = rod.attachments[index].Stack;
                            }
                        }

                        copy = rodCopy;
                    }
                    else
                    {
                        copy = item.getOne();
                    }

                    copy.Stack = item.Stack;

                    this.inventory.Add(copy);
                }
        }
    }
}