using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace BetterFruitTrees
{
    /***
     * Contains all the logic needed to allow for JunimoHuts to harvest FruitTrees.
     ***/
    class JunimoHutModifier : IInitializable
    {

        public void Init()
        {
            SaveEvents.BeforeSave += this.RevertToRegular;
            TimeEvents.AfterDayStarted += this.ConvertToFruitAware;
            MenuEvents.MenuClosed += this.MenuClosed;
        }

        /***
        * Converts all the JunimoHuts in the Farm from JunimoHut to FruitTreeAwareJunimoHut, after the carpenter menu closes (the player possibly built a new JunimoHut)
        ***/
        private void MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is CarpenterMenu)
                ConvertToFruitAware(sender, e);
        }

        /***
         * Converts all the JunimoHuts in the Farm from JunimoHut to FruitTreeAwareJunimoHut, after the game saves/a day starts to be able to harvest fruit trees.
         ***/
        private void ConvertToFruitAware(object sender, EventArgs e)
        {
            ConvertJunimoHuts<JunimoHut, FruitTreeAwareJunimoHut>();
        }

        /***
         * Converts all the JunimoHuts in the Farm from FruitTreeAwareJunimoHut to JunimoHut, before the game saves to leave the new object out of the save file.
         ***/
        private void RevertToRegular(object sender, EventArgs e)
        {
            ConvertJunimoHuts<FruitTreeAwareJunimoHut, JunimoHut>();
        }

        /***
         * Converts all the JunimoHuts in the Farm from FromType to ToType
         ***/
        private static void ConvertJunimoHuts<FromType, ToType>() where ToType : JunimoHut where FromType : JunimoHut
        {

            Farm f = Game1.getFarm();
            List<Building> buildings = f.buildings;
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i] is FromType)
                {
                    FromType oldHut = (buildings[i] as FromType);

                    //Kill all junimos before removing the building.
                    foreach (JunimoHarvester harvester in oldHut.myJunimos)
                    {
                        f.characters.Remove(harvester);
                    }

                    oldHut.myJunimos.Clear();

                    ToType newHut = (ToType)Activator.CreateInstance(typeof(ToType), new object[] { new BluePrint("Junimo Hut"), new Vector2(oldHut.tileX, oldHut.tileY) });
                    newHut.daysOfConstructionLeft = oldHut.daysOfConstructionLeft;
                    newHut.output = oldHut.output;
                    buildings[i] = newHut;
                }

            }
        }
    }
}


