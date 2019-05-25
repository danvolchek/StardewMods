using System.Linq;
using BetterSlingshots.Framework.Config;
using BetterSlingshots.Framework.Patching;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace BetterSlingshots.Framework
{
    internal class SlingshotManager
    {
        private readonly IReflectionHelper reflectionHelper;
        private readonly BetterSlingshotsModConfig config;
        private readonly PatchManager patchManager;

        private SObject lastFiredAmmo;

        public SlingshotManager(IReflectionHelper reflectionHelper, BetterSlingshotsModConfig config, PatchManager patchManager)
        {
            this.reflectionHelper = reflectionHelper;
            this.config = config;
            this.patchManager = patchManager;

            patchManager.BeforeFiring += this.BeforeFiring;
            patchManager.AfterFiring += this.AfterFiring;
            patchManager.AfterUsing += this.AfterUsing;
        }

        private void AfterUsing(object sender, SlingshotFiringEventArgs e)
        {
            if(this.config.CanMoveWhileFiring)
                e.Slingshot.getLastFarmerToUse().CanMove = true;
        }

        private void BeforeFiring(object sender, SlingshotFiringEventArgs e)
        {
            this.reflectionHelper.GetField<bool>(e.Slingshot, "canPlaySound").SetValue(false);

            this.lastFiredAmmo = e.Slingshot.attachments[0];
        }

        private void AfterFiring(object sender, SlingshotFiringEventArgs e)
        {
            if (this.config.InfiniteAmmo && this.lastFiredAmmo != null)
            {
                this.lastFiredAmmo.Stack++;

                if (this.lastFiredAmmo.Stack == 1)
                    e.Slingshot.attachments[0] = this.lastFiredAmmo;
            }

            if (this.config.AutoReload && this.lastFiredAmmo != null && this.lastFiredAmmo.Stack == 0)
            {
                this.Reload(e.Slingshot);
            }
        }

        private void Reload(Tool slingshot)
        {
            Item matchingItem = Game1.player.Items.FirstOrDefault(item => item != null && item.ParentSheetIndex == this.lastFiredAmmo.ParentSheetIndex);

            if (matchingItem is SObject obj)
            {
                slingshot.attachments[0] = obj;
                Game1.player.Items[Game1.player.Items.IndexOf(matchingItem)] = null; 
            }
        }

        public void FireSlingshot(Slingshot slingshot)
        {
            if (slingshot.attachments[0] == null)
            {
                return;
            }

            this.patchManager.ShouldFinishRun(slingshot, false);
            slingshot.DoFunction(Game1.currentLocation, Game1.getMouseX(), Game1.getMouseY(), 1, slingshot.getLastFarmerToUse());
            this.patchManager.ShouldFinishRun(slingshot, true);
        }
    }
}
