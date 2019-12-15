using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoStacker
{
    internal class AutoStackerMod : Mod
    {
        private AutoStackerConfig config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.config = helper.ReadConfig<AutoStackerConfig>();
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == this.config.ActivationKey)
            {
                this.StackOwnInventory();
            }
        }

        private void StackOwnInventory()
        {
            IList<Item> items = Game1.player.Items.Where(it => it != null && it.maximumStackSize() != -1).Reverse().ToList();
            foreach (Item item in items)
            {
                if (item.Stack == item.maximumStackSize() || item.Stack == 0)
                    continue;

                foreach (Item stackOnMe in Game1.player.Items.Where(it => it != null && it != item && it.canStackWith(item) && it.getRemainingStackSpace() > 0 && it.Stack != 0))
                {
                    int remain = stackOnMe.getRemainingStackSpace();
                    int add = Math.Min(remain, item.Stack);

                    stackOnMe.Stack += add;
                    item.Stack -= add;

                    if (item.Stack == 0)
                        break;
                }
            }

            for (int i = 0; i < Game1.player.Items.Count; i++)
            {
                Item it = Game1.player.Items[i];
                if (it != null && it.Stack == 0 && it.maximumStackSize() != -1)
                {
                    this.SwapStack(this.FindFirstNonEmptyStack(it, i + 1), it);
                }
            }

            for (int i = 0; i < Game1.player.Items.Count; i++)
            {
                Item it = Game1.player.Items[i];
                if (it != null && it.Stack == 0 && it.maximumStackSize() != -1)
                {
                    Game1.player.Items[i] = null;
                }
            }
        }

        private Item FindFirstNonEmptyStack(Item a, int from)
        {
            for (int i = from; i < Game1.player.Items.Count; i++)
            {
                Item it = Game1.player.Items[i];
                if (it != null && it.Stack != 0 && it.canStackWith(a))
                {
                    return it;
                }
            }
            return null;
        }

        private void SwapStack(Item a, Item b)
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
