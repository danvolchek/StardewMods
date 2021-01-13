using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace StackEverything.Patches
{
    internal class TryToPlaceItemPatch
    {
        /// <summary>The same as <see cref="Utility.tryToPlaceHere"/> except the furniture code is commented out.</summary>
        /// <remarks> Hopefully this doesn't cause any problems. I have no idea why it was added in 1.3.</remarks>
        public static bool Prefix(ref bool result, GameLocation location, Item item, int x, int y)
        {
            result = TryToPlaceItem(location, item, x, y);
            return false;
        }

        private static bool TryToPlaceItem(GameLocation location, Item item, int x, int y)
        {
            if (item == null || item is Tool)
                return false;
            var key = new Vector2(x / 64, y / 64);
            if (Utility.playerCanPlaceItemHere(location, item, x, y, Game1.player))
            {
                //if (item is Furniture)
                //    Game1.player.ActiveObject = (Object)null;
                if (((Object)item).placementAction(location, x, y, Game1.player))
                    Game1.player.reduceActiveItemByOne();
                else switch (item)
                {
                    case Furniture furniture:
                        Game1.player.ActiveObject = furniture;
                        break;
                    case Wallpaper _:
                        return false;
                }
                return true;
            }
            if (Utility.isPlacementForbiddenHere(location) && item.isPlaceable())
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
            else if (item is Furniture furniture)
            {
                switch (furniture.GetAdditionalFurniturePlacementStatus(location, x, y, Game1.player))
                {
                    case 1:
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12629"));
                        break;
                    case 2:
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"));
                        break;
                    case 3:
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12633"));
                        break;
                    case 4:
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"));
                        break;
                }
            }

            if (item.Category != -19 || !location.terrainFeatures.ContainsKey(key) ||
                !(location.terrainFeatures[key] is HoeDirt)) return false;
            var terrainFeature = location.terrainFeatures[key] as HoeDirt;
            if (((HoeDirt) location.terrainFeatures[key]).fertilizer.Value != 0)
            {
                if (((HoeDirt) location.terrainFeatures[key]).fertilizer.Value != item.ParentSheetIndex)
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916-2"));
                return false;
            }

            if (terrainFeature != null && (item.ParentSheetIndex != 368 && item.ParentSheetIndex != 368 || terrainFeature.crop == null || terrainFeature.crop.currentPhase.Value == 0)) return false;
            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"));
            return false;
        }
    }
}