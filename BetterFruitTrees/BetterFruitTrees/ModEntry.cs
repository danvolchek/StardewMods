using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace BetterFruitTrees
{
    public class ModEntry : Mod
    {

        public override void Entry(IModHelper helper)
        {
            if (helper.ModRegistry.IsLoaded("cat.fruittreesanywhere"))
            {
                Monitor.Log("You have both this mod, and the old version ('Fruit Trees Anywhere') installed!", LogLevel.Error);
                Monitor.Log("In order for this mod to work properly, you need to delete the FruitTreesAnywhere folder!", LogLevel.Error);
                Monitor.Log("This mod does everything the old version does and fruit tree junimo harvesting, so please delete FruitTreesAnywhere!", LogLevel.Error);
                SaveEvents.AfterLoad += this.ShowErrorMessage;
                return;
            }
            ModConfig config = helper.ReadConfig<ModConfig>();
            FruitTreeAwareJunimoHut.helper = helper;
            FruitTreeAwareJunimoHut.HarvestThreeAtOnce = config.Wait_To_Harvest_Fruit_Trees_Until_They_Have_Three_Fruits__Then_Harvest_All_Three_At_Once;
            FruitTreeAwareJunimoHarvester.helper = helper;

            IInitializable pHelper = new PlacementHelper();
            pHelper.Init();

            if (!config.Disable_Fruit_Tree_Junimo_Harvesting)
            {
                IInitializable modifier = new JunimoHutModifier();
                modifier.Init();
            }


            //Things to do
            //1. trees grow anyway (ez pz)
            //2.upgrade move building dialog(hard probs)

            //for this mod - just test and see if it works.

        }

        private void ShowErrorMessage(object sender, EventArgs e)
        {
            Game1.showRedMessage("Better Fruit Trees failed to load - please see the console for how to fix this.");
        }


    }
}
