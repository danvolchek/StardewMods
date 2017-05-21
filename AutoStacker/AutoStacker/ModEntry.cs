using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using StardewModdingAPI.Events;

namespace AutoStacker
{
    class ModEntry : Mod
    {
        private ModConfig config;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            ControlEvents.KeyPressed += this.KeyPressed;
        }

        private void KeyPressed(object sender, EventArgsKeyPressed e)
        {
            String key = e.KeyPressed.ToString().ToLower();

            if (key == config.ActivationKey.ToLower())
            {
               /* List<Item> items = Game1.player.items.FindAll(it => it != null);
                foreach (Item item in items)
                {
                    Monitor.Log(item.Name + ": " + item.Stack+"/"+item.maximumStackSize());
                }*/
                stackOwnInventory();

            }
        }

        private void stackOwnInventory()
        {
            List<Item> items = Game1.player.items.FindAll(it => it != null && it.maximumStackSize()!=-1);
            items.Reverse();
            foreach (Item item in items)
            {
               
                if (item.Stack == item.maximumStackSize() || item.Stack == 0)
                    continue;

                foreach (Item stackOnMe in Game1.player.items.FindAll(it => it != null && it != item && it.canStackWith(item) && it.getRemainingStackSpace() > 0))
                {
                    int remain = stackOnMe.getRemainingStackSpace();
                    int add = Math.Min(remain, item.Stack);
                 
                    stackOnMe.addToStack(add);
                    item.Stack -= add;

                    if (item.Stack == 0)
                        break;

                }
            }

            for (int i = 0; i < Game1.player.items.Count; i++)
            {
                Item it = Game1.player.items[i];
                if (it != null && it.Stack == 0 && it.maximumStackSize() != -1)
                {
                    swapStack(findFirstNonEmptyStack(it, i + 1), it);

                }
            }

            for (int i = 0; i < Game1.player.items.Count; i++)
            {
                Item it = Game1.player.items[i];
                if (it != null && it.Stack == 0 && it.maximumStackSize() != -1)
                {
                    Game1.player.items[i] = null;

                }
            }



        }

        private Item findFirstNonEmptyStack(Item a, int from)
        {
            for (int i = from; i < Game1.player.items.Count; i++)
            {
                Item it = Game1.player.items[i];
                if (it != null && it.Stack != 0 && it.canStackWith(a))
                {
                    return it;
                }
            }
            return null;
        }

        private void swapStack(Item a, Item b)
        {
            if (a == null || b == null)
                return;
            if (!a.canStackWith(b))
                return;
            int temp = a.Stack;
            a.Stack = b.Stack;
            b.Stack = temp;
        }
    }
}