using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterFruitTrees.Patches;
using BetterFruitTrees.Patches.JunimoHarvester;
using BetterFruitTrees.Patches.JunimoHut;
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using static BetterFruitTrees.Extensions.ListExtensions;
using SObject = StardewValley.Object;

namespace BetterFruitTrees
{
    public class ModEntry : Mod
    {
        public static ModEntry Instance;
        internal ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            Utils.Reflection = helper.Reflection;
            Config = Helper.ReadConfig<ModConfig>();
            new GrowHelper(helper.Events);
            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            Utils.HarvestThreeAtOnce = Config.WaitToHarvestFruitTreesUntilTheyHaveThreeFruitsThenHarvestAllThreeAtOnce;
            IList<Tuple<string, Type, Type>> replacements = new List<Tuple<string, Type, Type>>
            {
                {nameof(SObject.placementAction), typeof(SObject), typeof(PlacementPatch)}
            };
            var junimoHarvesterType = typeof(JunimoHarvester);
            IList<Tuple<string, Type, Type>> junimoReplacements = new List<Tuple<string, Type, Type>>
            {
                {nameof(JunimoHarvester.tryToHarvestHere), junimoHarvesterType, typeof(TryToHarvestHerePatch)},
                {nameof(JunimoHarvester.update), junimoHarvesterType, typeof(UpdatePatch)},
                {"areThereMatureCropsWithinRadius", typeof(JunimoHut), typeof(AreThereMatureCropsWithinRadiusPatch)}
            };
            if (!Config.DisableFruitTreeJunimoHarvesting)
                foreach (var item in junimoReplacements)
                    replacements.Add(item);
            foreach (var replacement in replacements)
            {
                var original = replacement.Item2
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList()
                    .Find(m => m.Name == replacement.Item1);
                var prefix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(item => item.Name == "Prefix");
                var postfix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(item => item.Name == "Postfix");
                harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix),
                    postfix == null ? null : new HarmonyMethod(postfix));
            }
        }

        private static void ShowErrorMessage(object sender, EventArgs e)
        {
            Game1.showRedMessage("Better Fruit Trees failed to load - please see the console for how to fix this.");
        }
    }
}