using System;
using System.Reflection;
using BetterSlingshots.Framework.Patching.Patches;
using Harmony;
using StardewValley;
using StardewValley.Tools;

namespace BetterSlingshots.Framework.Patching
{
    internal class PatchManager
    {

        public static PatchManager Instance;

        public PatchManager(string harmonyId)
        {
            PatchManager.Instance = this;

            HarmonyInstance harmony = HarmonyInstance.Create(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

        public event EventHandler<SlingshotFiringEventArgs> BeforeFiring;
        public event EventHandler<SlingshotFiringEventArgs> AfterUsing;
        public event EventHandler<SlingshotFiringEventArgs> AfterFiring;

        public void BeforeFiringHook(Slingshot slingshot)
        {
            if(slingshot.getLastFarmerToUse() == Game1.player)
                this.BeforeFiring?.Invoke(null, new SlingshotFiringEventArgs(slingshot));
        }

        public void AfterFiringHook(Slingshot slingshot)
        {
            if (slingshot.getLastFarmerToUse() == Game1.player)
                this.AfterFiring?.Invoke(null, new SlingshotFiringEventArgs(slingshot));
        }

        public void AfterUsingHook(Slingshot slingshot)
        {
            if (slingshot.getLastFarmerToUse() == Game1.player)
                this.AfterUsing?.Invoke(null, new SlingshotFiringEventArgs(slingshot));
        }

        public void ShouldFinishRun(Slingshot instance, bool shouldRun)
        {
            SlingshotFinishPatch.ShouldRun(instance, shouldRun);
        }
    }
}
