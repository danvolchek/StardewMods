using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterGardenPots.Extensions;
using BetterGardenPots.Patches.IndoorPot;
using BetterGardenPots.Patches.Utility;
using BetterGardenPots.Subscribers;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace BetterGardenPots
{
    public class BetterGardenPotsMod : Mod
    {
        private readonly IList<IEventSubscriber> subscribers = new List<IEventSubscriber>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            HarmonyInstance harmony = HarmonyInstance.Create("cat.bettergardenpots");

            BetterGardenPotsModConfig config = helper.ReadConfig<BetterGardenPotsModConfig>();

            if (config.MakeSprinklersWaterGardenPots) this.subscribers.Add(new GardenPotSprinklerHandler(this.Helper));

            Type indoorPotType = typeof(IndoorPot);

            IList<Tuple<string, Type, Type>> replacements = new List<Tuple<string, Type, Type>>();

            if (config.MakeBeeHousesNoticeFlowersInGardenPots)
                replacements.Add(nameof(Utility.findCloseFlower), typeof(Utility), typeof(FindCloseFlowerPatch));

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
                MethodInfo original = replacement.Item2.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == replacement.Item1);

                MethodInfo prefix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Prefix");
                MethodInfo postfix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Postfix");

                harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix), postfix == null ? null : new HarmonyMethod(postfix));
            }

            SaveEvents.AfterReturnToTitle += this.SaveEvents_AfterReturnToTitle;
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            if (Context.IsMainPlayer)
                foreach (IEventSubscriber subscriber in this.subscribers)
                    subscriber.Subscribe();
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            foreach (IEventSubscriber subscriber in this.subscribers)
                subscriber.Unsubscribe();
        }
    }
}
