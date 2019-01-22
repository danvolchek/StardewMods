using System;
using System.Collections.Generic;
using System.Linq;
using GeodeInfoMenu.Menus;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace GeodeInfoMenu
{
    /// <summary>
    /// Represents the geode info menu mod.
    /// </summary>
    public class GeodeInfoMenuMod : Mod
    {
        /***
         * Private Fields
         ***/

        /// <summary>
        /// A mapping of GeodeType to item id of that geode.
        /// </summary>
        private IDictionary<GeodeType, int> Geodes;

        /// <summary>
        /// A mapping of item name to that GeodeDrop.
        /// </summary>
        private IDictionary<string, GeodeDrop> dropNameToGeodeDrop;

        /// <summary>
        /// The saved state info of the last opened geode menu.
        /// </summary>
        private GeodeMenuStateInfo menuStateInfo;

        /// <summary>
        /// The mod configuration.
        /// </summary>
        private GeodeInfoMenuConfig config;

        /// <summary>
        /// The menu where players actually break geodes.
        /// </summary>
        private StardewValley.Menus.IClickableMenu GeodeBreakingMenu = null;


        /// <summary>
        /// Entry method. Sets up config and event listeners.
        /// </summary>
        /// <param name="helper">Mod helper to read config and load sprites.</param>
        public override void Entry(IModHelper helper)
        {
            this.config = helper.ReadConfig<GeodeInfoMenuConfig>();
            this.config.NumberOfNextGeodeDropsToShow = this.config.NumberOfNextGeodeDropsToShow < 0 ? 0 : this.config.NumberOfNextGeodeDropsToShow;
            this.config.NumberOfNextGeodeDropsToShow = this.config.NumberOfNextGeodeDropsToShow > 999 ? 9 : this.config.NumberOfNextGeodeDropsToShow;
            this.Geodes = new Dictionary<GeodeType, int> {
                {GeodeType.Normal, 535}, {GeodeType.FrozenGeode, 536}, {GeodeType.MagmaGeode, 537}, {GeodeType.OmniGeode, 749}
            };
            this.dropNameToGeodeDrop = GetAllPossibleDropMappings();
            GeodeMenu.tabIcons = helper.Content.Load<Texture2D>("Sprites/tabs.png");
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Display.WindowResized += this.OnWindowResized;

        }

        /***
        ** Event Listeners
        ****/
        /// <summary>Raised after the game window is resized.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWindowResized(object sender, EventArgs e)
        {
            // recreate menu to fit new window size
            if (Game1.activeClickableMenu is GeodeMenu menu)
            {
                this.menuStateInfo = menu.SaveState();
                Game1.activeClickableMenu = new GeodeMenu(this, this.config, GetNextDropsForGeodes(this.config.NumberOfNextGeodeDropsToShow), this.menuStateInfo, true);
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // save the last geode menu state
            SaveMenuState(e.OldMenu);
            this.GeodeBreakingMenu = null;
        }

        /// <summary>
        /// Saves the menu state from e if e is a geode menu.
        /// </summary>
        /// <param name="e">A menu</param>
        public void SaveMenuState(StardewValley.Menus.IClickableMenu e)
        {
            if (e is GeodeMenu menu)
                this.menuStateInfo = menu.SaveState();
        }

        /// <summary>
        /// Gets the last opened menu that is not a GeodeMenu.
        /// </summary>
        public StardewValley.Menus.IClickableMenu GetLastMenu()
        {
            return this.GeodeBreakingMenu;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // open the menu if activation key pressed
            bool canOpen = Game1.activeClickableMenu == null || Game1.activeClickableMenu is StardewValley.Menus.GeodeMenu;
            if (Game1.activeClickableMenu is StardewValley.Menus.GeodeMenu)
            {
                this.GeodeBreakingMenu = Game1.activeClickableMenu;
            }
            if (e.Button == this.config.ActivationKey && canOpen)
            {
                GeodeMenu menu = new GeodeMenu(this, this.config, GetNextDropsForGeodes(this.config.NumberOfNextGeodeDropsToShow), this.config.RememberMenuStateAfterClose ? this.menuStateInfo : null);
                Game1.activeClickableMenu = menu;
                menu.SetSearchTabSearchBoxSelectedStatus(true);
            }
        }

        /***
         * Public Methods
         ***/

        /// <summary>
        /// Gets all gede drops that contain a given string.
        /// </summary>
        /// <param name="partialName">The string to search for in each geode drop</param>
        /// <returns>A list of items</returns>
        public IList<GeodeDrop> GetItemsFromString(string partialName)
        {
            IList<GeodeDrop> items = new List<GeodeDrop>();

            foreach (KeyValuePair<string, GeodeDrop> kvp in this.dropNameToGeodeDrop)
                if (kvp.Key.Contains(partialName))
                    items.Add(kvp.Value);
            return items;
        }

        /// <summary>
        /// Gathers all the information needed to display a search result item.
        /// </summary>
        /// <param name="item">The item to gather information for.</param>
        /// <param name="searchResultInfo">Output of the geodes to crack and how many of them it will take</param>
        /// <param name="showStar">Whether or not to show a new star with the item</param>
        /// <returns>Whether this item can be dropped from geodes</returns>
        public bool GetInfoToBuildSearchResult(GeodeDrop item, out Tuple<int, int>[] searchResultInfo, out bool showStar)
        {
            bool[] geodesToCrack = GetCrackedGeodeFromWantedItem(item);
            if (geodesToCrack == null)
            {
                searchResultInfo = null;
                showStar = false;
                return false;
            }
            searchResultInfo = new Tuple<int, int>[geodesToCrack.Count(elem => elem)];
            int currIndex = 0;
            for (int i = 0; i < 4; i++)
            {
                if (geodesToCrack[i])
                {
                    int geodeParentSheetIndex = this.Geodes[GetGeodeTypes()[i]];
                    int amnt = GeodesUntilTreasure(geodeParentSheetIndex, item.ParentSheetIndex);
                    searchResultInfo[currIndex++] = new Tuple<int, int>(geodeParentSheetIndex, amnt);
                }
            }
            showStar = this.HasNotDonatedItemToMuseum(item.ParentSheetIndex);
            return true;
        }

        /***
         * Private Methods
         ***/

        /// <summary>
        /// Gets the geodes that can be cracked to get a given item.
        /// </summary>
        /// <param name="item">The drop to look for.</param>
        /// <returns>A bool array in order of the GeodeType enum indicating whether that geode can be used.</returns>
        private bool[] GetCrackedGeodeFromWantedItem(GeodeDrop item)
        {
            if (item.IsHardCodedDrop)
                switch (item.HardCodedDrop)
                {
                    case HardCodedGeodeDrop.EarthCrystal:
                        return new bool[] { true, false, false, false };

                    case HardCodedGeodeDrop.FrozenTear:
                        return new bool[] { false, true, false, false };

                    case HardCodedGeodeDrop.FireQuartz:
                        return new bool[] { false, false, true, true };

                    case HardCodedGeodeDrop.Stone:
                    case HardCodedGeodeDrop.Clay:
                    case HardCodedGeodeDrop.CopperOre:
                    case HardCodedGeodeDrop.Coal:
                        return new bool[] { true, true, true, true };

                    case HardCodedGeodeDrop.IronOre:
                        return new bool[] { Game1.player.deepestMineLevel > 25, true, true, true };

                    case HardCodedGeodeDrop.GoldOre:
                        return new bool[] { false, Game1.player.deepestMineLevel > 75, true, true };

                    case HardCodedGeodeDrop.IridiumOre:
                        return new bool[] { false, false, true, true };

                    case HardCodedGeodeDrop.PrismaticShard:
                        return new bool[] { false, false, false, true };
                    default:
                        return null;
                }
            else
            {
                bool[] acceptable = new bool[] { false, false, false, false };
                GeodeType[] types = GetGeodeTypes();
                for (int i = 0; i < types.Length; i++)
                    foreach (int drop in GetDropsFromGeode(types[i]))
                        if (drop == item.ParentSheetIndex)
                        {
                            acceptable[i] = true;
                            break;
                        }
                return acceptable;
            }
        }

        /// <summary>
        /// Gets all possible drops that can be gotten from geodes.
        /// </summary>
        /// <returns>A mapping of the item name to a corresponding GeodeDrop</returns>
        private IDictionary<string, GeodeDrop> GetAllPossibleDropMappings()
        {
            IDictionary<string, GeodeDrop> mapping = new Dictionary<string, GeodeDrop>();
            foreach (HardCodedGeodeDrop item in ((HardCodedGeodeDrop[])Enum.GetValues(typeof(HardCodedGeodeDrop))))
            {
                GeodeDrop geodeDrop = new GeodeDrop(item);
                mapping.Add(Game1.objectInformation[geodeDrop.ParentSheetIndex].Split('/')[0].ToLower(), geodeDrop);
            }
            foreach (GeodeType type in GetGeodeTypes())
            {
                foreach (string drop in Game1.objectInformation[this.Geodes[type]].Split('/')[6].Split(' '))
                {
                    string name = Game1.objectInformation[Convert.ToInt32(drop)].Split('/')[0].ToLower();
                    if (!mapping.ContainsKey(name))
                        mapping.Add(name, new GeodeDrop(Convert.ToInt32(drop)));

                }
            }
            return mapping;
        }

        /// <summary>
        /// Gets all the item ids that drop from a geode type.
        /// </summary>
        /// <param name="type">The type of geode to look up</param>
        /// <returns>An integer array of item ids</returns>
        private int[] GetDropsFromGeode(GeodeType type)
        {
            return Array.ConvertAll(Game1.objectInformation[this.Geodes[type]].Split('/')[6].Split(' '), s => int.Parse(s));
        }

        /// <summary>
        /// Gets the next drops for each geode type.
        /// </summary>
        /// <param name="amount">How many drops to get</param>
        /// <returns>A list for each geode type containing tuples of item ids and whether to show  star for that item id</returns>
        private IList<Tuple<int[], bool[]>> GetNextDropsForGeodes(int amount)
        {
            IList<Tuple<int[], bool[]>> list = new List<Tuple<int[], bool[]>>();

            foreach (GeodeType type in GetGeodeTypes())
            {
                int GeodesCracked = (int)Game1.stats.GeodesCracked + 1;
                int[] items = new int[amount];
                bool[] stars = new bool[amount];
                for (int i = 0; i < amount; i++)
                {
                    items[i] = GeodeSimulator(this.Geodes[type], GeodesCracked++);
                    stars[i] = this.HasNotDonatedItemToMuseum(items[i]);
                }
                list.Add(new Tuple<int[], bool[]>(items, stars));
            }
            return list;
        }

        /// <summary>
        /// Returns whether an item has already been donated to the museum.
        /// </summary>
        /// <param name="parentSheetIndex">The item to look up</param>
        /// <returns>Whether this item has been donated or not</returns>
        private bool HasNotDonatedItemToMuseum(int parentSheetIndex)
        {
            if (!this.config.ShowStarsNextToMineralsAndArtifactsNotDonatedToTheMuseum)
                return false;
            string objectInfo = Game1.objectInformation[parentSheetIndex].Split('/')[3];
            return (objectInfo.Contains("Mineral") || objectInfo.Contains("Arch")) &&
                !Game1.locations.OfType<LibraryMuseum>().First().museumAlreadyHasArtifact(parentSheetIndex);
        }

        /// <summary>
        /// Simulates Geode opening until it pulls the item you want.
        /// </summary>
        /// <param name="geodeBeingCracked">The item id of the geode being cracked</param>
        /// <param name="wantedItem">The item id of the wanted item</param>
        /// <returns>The number of geodes needed to get that item</returns>
        private int GeodesUntilTreasure(int geodeBeingCracked, int wantedItem)
        {
            int GeodesCracked = (int)Game1.stats.GeodesCracked + 1;
            int tries = 0;
            while (tries++ < 1000 && GeodeSimulator(geodeBeingCracked, GeodesCracked++) != wantedItem) ;

            return tries;
        }

        /// <summary>
        /// Simulates opening the given geode.
        /// </summary>
        /// <param name="parentSheetIndex">The geode to crack open</param>
        /// <param name="GeodesCracked">How many geodes have already been cracked this game</param>
        /// <returns>The item id of the item that comes out of the geode</returns>
        private int GeodeSimulator(int parentSheetIndex, int GeodesCracked)
        {
            try
            {
                Random random = new Random((int)GeodesCracked + (int)Game1.uniqueIDForThisGame / 2);
                if (random.NextDouble() < 0.5)
                {
                    int initialStack = random.Next(3) * 2 + 1;
                    if (random.NextDouble() < 0.1)
                        initialStack = 10;
                    if (random.NextDouble() < 0.01)
                        initialStack = 20;
                    if (random.NextDouble() < 0.5)
                    {
                        switch (random.Next(4))
                        {
                            case 0:
                            case 1:
                                return 390;//stone
                            case 2:
                                return 330;//clay
                            case 3:
                                switch (parentSheetIndex)
                                {
                                    case 535:
                                        return 86; //earth crystal
                                    case 536:
                                        return 84; //frozen tear
                                    case 749:
                                        return 82 + random.Next(3) * 2; //fire quartz, frozen tear, earth crystal
                                    default:
                                        return 82; //fire quartz
                                }
                        }
                    }
                    else if (parentSheetIndex == 535) //geode
                    {
                        switch (random.Next(3))
                        {
                            case 0:
                                return 378; //copper ore
                            case 1:
                                return Game1.player.deepestMineLevel > 25 ? 380 : 378; // iron ore : copper ore
                            case 2:
                                return 382; //coal
                        }
                    }
                    else if (parentSheetIndex == 536) //frozen geode
                    {
                        switch (random.Next(4))
                        {
                            case 0:
                                return 378; //copper ore
                            case 1:
                                return 380; //iron ore
                            case 2:
                                return 382; //coal
                            case 3:
                                return Game1.player.deepestMineLevel > 75 ? 384 : 380; // gold ore : coal
                        }
                    }
                    else
                    {
                        switch (random.Next(5))
                        {
                            case 0:
                                return 378; //copper ore
                            case 1:
                                return 380; //iron ore
                            case 2:
                                return 382; //coal
                            case 3:
                                return 384; //gold ore
                            case 4:
                                return 386; // iridium ore
                        }
                    }
                }
                else
                {
                    string[] strArray = Game1.objectInformation[parentSheetIndex].Split('/')[6].Split(' ');
                    int int32 = Convert.ToInt32(strArray[random.Next(strArray.Length)]);
                    if (parentSheetIndex == 749 && random.NextDouble() < 0.008 && (int)GeodesCracked > 15)
                        return 74;
                    return int32;
                }
            }
            catch (Exception)
            {
            }
            return 390;
        }

        /// <summary>
        /// Converts a bool array of geodes to an int array of geode item ids
        /// </summary>
        /// <param name="geodes">Array indicaing which geodes to get an item id for</param>
        /// <returns>An array of item ids</returns>
        private int[] ConvertBoolArrayOfGeodesToIntArray(bool[] geodes)
        {
            int[] iGeodes = new int[geodes.Count(item => item)];
            int iGeodeIndex = 0;
            for (int i = 0; i < geodes.Length; i++)
                if (geodes[i])
                    iGeodes[iGeodeIndex++] = this.Geodes[GetGeodeTypes()[i]];
            return iGeodes;
        }

        /// <summary>
        /// Gets all the types of geodes.
        /// </summary>
        /// <returns></returns>
        private GeodeType[] GetGeodeTypes()
        {
            return ((GeodeType[])(Enum.GetValues(typeof(GeodeType))));
        }
    }
}
