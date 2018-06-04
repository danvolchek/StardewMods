using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterGardenPots.Patches.Utility;
using Harmony;
using StardewModdingAPI;
using BetterGardenPots.Extensions;
using BetterGardenPots.Patches.IndoorPot;
using BetterGardenPots.Subscribers;
using StardewModdingAPI.Events;

namespace BetterGardenPots
{
    public class BetterGardenPotsMod : Mod
    {
        private readonly IList<IEventSubscriber> subscribers = new List<IEventSubscriber>();

        public override void Entry(IModHelper helper)
        {
            HarmonyInstance harmony = HarmonyInstance.Create("cat.bettergardenpots");

            BetterGardenPotsModConfig config = helper.ReadConfig<BetterGardenPotsModConfig>();

            if (config.MakeSprinklersWaterGardenPots) this.subscribers.Add(new GardenPotSprinklerHandler(this.Helper));

            Type indoorPotType = GetSDVType("Objects.IndoorPot");

            IList<Tuple<string, Type, Type>> replacements = new List<Tuple<string, Type, Type>>();

            if (config.MakeBeeHousesNoticeFlowersInGardenPots)
                replacements.Add("findCloseFlower", GetSDVType("Utility"), typeof(FindCloseFlowerPatch));

            if (config.HarvestMatureCropsWhenGardenPotBreaks)
                replacements.Add("performToolAction", indoorPotType, typeof(PerformToolActionPatch));

            if (config.AllowPlantingAncientSeedsInGardenPots)
                replacements.Add("performObjectDropInAction", indoorPotType,
                    typeof(PerformObjectDropInActionPatchFruit));

            if (config.AllowCropsToGrowInAnySeasonOutsideWhenInGardenPot)
            {
                replacements.Add("DayUpdate", indoorPotType, typeof(DayUpdatePatch));
                replacements.Add("performObjectDropInAction", indoorPotType,
                    typeof(PerformObjectDropInActionPatchSeasons));
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

        //Big thanks to Routine for this workaround for mac users.
        //https://github.com/Platonymous/Stardew-Valley-Mods/blob/master/PyTK/PyUtils.cs#L117
        /// <summary>Gets the correct type of the object, handling different assembly names for mac/linux users.</summary>
        private static Type GetSDVType(string type)
        {
            const string prefix = "StardewValley.";
            Type defaultSDV = Type.GetType(prefix + type + ", Stardew Valley");

            return defaultSDV ?? Type.GetType(prefix + type + ", StardewValley");
        }
    }
}
