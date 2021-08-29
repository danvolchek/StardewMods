using BetterGardenPots.Extensions;
using BetterGardenPots.Patches.IndoorPot;
using BetterGardenPots.Patches.Utility;
using BetterGardenPots.Subscribers;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterGardenPots
{
    public class BetterGardenPotsMod : Mod
    {
        private readonly IList<IEventSubscriber> subscribers = new List<IEventSubscriber>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Harmony harmony = new Harmony("cat.bettergardenpots");

            BetterGardenPotsModConfig config = helper.ReadConfig<BetterGardenPotsModConfig>();

            if (config.MakeSprinklersWaterGardenPots) this.subscribers.Add(new GardenPotSprinklerHandler(this.Helper));

            Type indoorPotType = typeof(IndoorPot);

            IList<Tuple<string, Type, Type>> replacements = new List<Tuple<string, Type, Type>>();

            if (config.MakeBeeHousesNoticeFlowersInGardenPots)
            {
                // I'm assuming it used to not have an overload: Thus this. There's probably a saner option, but meh.
                var methodList = typeof(Utility).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
                replacements.Add(methodList.Find(m => (m.Name == "findCloseFlower" && m.GetParameters().Length == 4)).ToString(), typeof(Utility), typeof(FindCloseFlowerPatch));
            }
                

            if (config.HarvestMatureCropsWhenGardenPotBreaks)
                replacements.Add(nameof(IndoorPot.performToolAction), indoorPotType, typeof(PerformToolActionPatch));

            if (config.AllowPlantingAncientSeedsInGardenPots)
                replacements.Add(nameof(IndoorPot.performObjectDropInAction), indoorPotType, typeof(PerformObjectDropInActionPatchFruit));

            if (config.AllowCropsToGrowInAnySeasonOutsideWhenInGardenPot)
            {
                replacements.Add(nameof(IndoorPot.DayUpdate), indoorPotType, typeof(DayUpdatePatch));
                replacements.Add(nameof(IndoorPot.performObjectDropInAction), indoorPotType, typeof(PerformObjectDropInActionPatchSeasons));
            }

            foreach (Tuple<string, Type, Type> replacement in replacements)
            {
                MethodInfo original = null;
                // If there's a space, assume it's a full method signature, not just a name.
                if (replacement.Item1.Contains(" "))
                    original = replacement.Item2.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.ToString() == replacement.Item1);
                else
                    original = replacement.Item2.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == replacement.Item1);

                MethodInfo prefix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Prefix");
                MethodInfo postfix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Postfix");

                harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix), postfix == null ? null : new HarmonyMethod(postfix));
            }

            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, EventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                foreach (IEventSubscriber subscriber in this.subscribers)
                    subscriber.Subscribe();
            }
        }

        /// <summary>Raised after the game returns to the title screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            foreach (IEventSubscriber subscriber in this.subscribers)
                subscriber.Unsubscribe();
        }
    }
}
