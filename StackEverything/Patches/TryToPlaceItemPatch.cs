using Microsoft.Xna.Framework;
using Netcode;
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
        public static bool Prefix(ref bool __result, GameLocation location, Item item, int x, int y)
        {
            __result = TryToPlaceItem(location, item, x, y);
            return false;
        }

        private static bool TryToPlaceItem(GameLocation location, Item item, int x, int y)
        {
            if (item == null || item is Tool)
                return false;
            Vector2 key = new Vector2((float)(x / 64), (float)(y / 64));
            if (Utility.playerCanPlaceItemHere(location, item, x, y, Game1.player))
            {
                //if (item is Furniture)
                //    Game1.player.ActiveObject = (Object)null;
                if (((Object)item).placementAction(location, x, y, Game1.player))
                    Game1.player.reduceActiveItemByOne();
                else if (item is Furniture)
                    Game1.player.ActiveObject = (Object)(item as Furniture);
                else if (item is Wallpaper)
                    return false;
                return true;
            }
            if (Utility.isPlacementForbiddenHere(location) && item != null && item.isPlaceable())
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
            else if (item is Furniture)
            {
                switch ((item as Furniture).GetAdditionalFurniturePlacementStatus(location, x, y, Game1.player))
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
            if (item.Category == -19 && location.terrainFeatures.ContainsKey(key) && location.terrainFeatures[key] is HoeDirt)
            {
                HoeDirt terrainFeature = location.terrainFeatures[key] as HoeDirt;
                if ((int)((NetFieldBase<int, NetInt>)(location.terrainFeatures[key] as HoeDirt).fertilizer) != 0)
                {
                    if ((NetFieldBase<int, NetInt>)(location.terrainFeatures[key] as HoeDirt).fertilizer != item.parentSheetIndex)
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916-2"));
                    return false;
                }
                if (((int)((NetFieldBase<int, NetInt>)item.parentSheetIndex) == 368 || (int)((NetFieldBase<int, NetInt>)item.parentSheetIndex) == 368) && (terrainFeature.crop != null && (int)((NetFieldBase<int, NetInt>)terrainFeature.crop.currentPhase) != 0))
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"));
                    return false;
                }
            }
            return false;
        }
    }
}
