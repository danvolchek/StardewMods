using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterFruitTrees.Patches;
using BetterFruitTrees.Patches.JunimoHarvester;
using BetterFruitTrees.Patches.JunimoHut;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using static BetterFruitTrees.Extensions.ListExtensions;

namespace BetterFruitTrees
{
    public class BetterFruitTreesMod : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Utils.Reflection = helper.Reflection;
            if (helper.ModRegistry.IsLoaded("cat.fruittreesanywhere"))
            {
                this.Monitor.Log("You have both this mod, and the old version ('Fruit Trees Anywhere') installed!",
                    LogLevel.Error);
                this.Monitor.Log(
                    "In order for this mod to work properly, you need to delete the FruitTreesAnywhere folder!",
                    LogLevel.Error);
                this.Monitor.Log(
                    "This mod does everything the old version does and fruit tree junimo harvesting, so please delete FruitTreesAnywhere!",
                    LogLevel.Error);
                SaveEvents.AfterLoad += this.ShowErrorMessage;
                return;
            }

            BetterFruitTreesConfig config = helper.ReadConfig<BetterFruitTreesConfig>();

            IInitializable pHelper = new PlacementHelper(config);
            pHelper.Init();

            if (config.Disable_Fruit_Tree_Junimo_Harvesting)
                return;

            Utils.HarvestThreeAtOnce =
                config.Wait_To_Harvest_Fruit_Trees_Until_They_Have_Three_Fruits__Then_Harvest_All_Three_At_Once;

            HarmonyInstance harmony = HarmonyInstance.Create("cat.betterfruittrees");

            Type junimoHarvesterType = Utils.GetSDVType("Characters.JunimoHarvester");

            IList<Tuple<string, Type, Type>> replacements = new List<Tuple<string, Type, Type>>
            {
                {"foundCropEndFunction", junimoHarvesterType, typeof(FoundCropEndFunctionPatch)},
                {"tryToHarvestHere", junimoHarvesterType, typeof(TryToHarvestHerePatch)},
                {"update", junimoHarvesterType, typeof(UpdatePatch)},
                {
                    "areThereMatureCropsWithinRadius", Utils.GetSDVType("Buildings.JunimoHut"),
                    typeof(AreThereMatureCropsWithinRadiusPatch)
                }
            };

            foreach (Tuple<string, Type, Type> replacement in replacements)
            {
                MethodInfo original = replacement.Item2
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList()
                    .Find(m => m.Name == replacement.Item1);

                MethodInfo prefix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(item => item.Name == "Prefix");
                MethodInfo postfix = replacement.Item3.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(item => item.Name == "Postfix");

                harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix),
                    postfix == null ? null : new HarmonyMethod(postfix));
            }
        }

        private void ShowErrorMessage(object sender, EventArgs e)
        {
            Game1.showRedMessage("Better Fruit Trees failed to load - please see the console for how to fix this.");
        }
    }
}