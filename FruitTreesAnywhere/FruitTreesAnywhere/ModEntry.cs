using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FruitTreesAnywhere
{
    public class ModEntry : Mod
    {
        private int RemoveChecks = 0;

        public override void Entry(IModHelper helper)
        {
            SaveEvents.BeforeSave += this.BeforeSave;
            InputEvents.ButtonPressed += this.ButtonPressed;
            GameEvents.UpdateTick += this.UpdateTick;
            ControlEvents.MouseChanged += this.MouseChanged;
        }

        /***
         * The next four methods mimic the game's codepath when an action button is pressed,
         * but place a sapling if a sapling could not be placed due to it being too close to another tree.
         ***/
        private void ButtonPressed(object sender, EventArgsInput e)
        {
            if ((e.IsActionButton || e.IsUseToolButton) && Game1.player.ActiveObject != null && Game1.player.ActiveObject.name.Contains("Sapling"))
            {
                Monitor.Log("Holding Sapling");
                Vector2 vector2 = GetMouseTile();

                if (!Game1.eventUp || Game1.isFestival())
                {
                    if (Game1.tryToCheckAt(vector2, Game1.player))
                        return;
                    if (Game1.player.isRidingHorse())
                    {
                        Game1.player.getMount().checkAction(Game1.player, Game1.player.currentLocation);
                        return;
                    }
                    if (!Game1.player.canMove)
                        return;
                    if (Game1.player.ActiveObject != null && !(Game1.player.ActiveObject is Furniture))
                    {
                        int stack = Game1.player.ActiveObject.Stack;
                        TryToPlaceSapling(Game1.currentLocation, (Item)Game1.player.ActiveObject, (int)vector2.X * Game1.tileSize + Game1.tileSize / 2, (int)vector2.Y * Game1.tileSize + Game1.tileSize / 2);
                    }
                }

            }
        }

        private Vector2 GetMouseTile()
        {
            Vector2 vector2 = new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y)) / (float)Game1.tileSize;
            if ((double)Game1.mouseCursorTransparency == 0.0 || !Game1.wasMouseVisibleThisFrame || !Game1.lastCursorMotionWasMouse && (Game1.player.ActiveObject == null || !Game1.player.ActiveObject.isPlaceable() && Game1.player.ActiveObject.category != -74))
            {
                vector2 = Game1.player.GetGrabTile();
                if (vector2.Equals(Game1.player.getTileLocation()))
                    vector2 = Utility.getTranslatedVector2(vector2, Game1.player.facingDirection, 1f);
            }
            if (!Utility.tileWithinRadiusOfPlayer((int)vector2.X, (int)vector2.Y, 1, Game1.player))
            {
                vector2 = Game1.player.GetGrabTile();
                if (vector2.Equals(Game1.player.getTileLocation()) && Game1.isAnyGamePadButtonBeingPressed())
                    vector2 = Utility.getTranslatedVector2(vector2, Game1.player.facingDirection, 1f);
            }
            return vector2;
        }

        private void TryToPlaceSapling(GameLocation location, Item item, int x, int y)
        {
            if (item is Tool)
                return;
            Vector2 vector2 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            if (Utility.playerCanPlaceItemHere(location, item, x, y, Game1.player))
            {
                if (!HandleAttemptedSaplingPlacement(((StardewValley.Object)item), location, x, y, Game1.player))
                    return;
                if (Game1.IsClient)
                    Game1.client.sendMessage((byte)4, new object[5]
                    {
            (object) (short) x,
            (object) (short) y,
            (object) (byte) 0,
            (object) (byte) (((StardewValley.Object) item).bigCraftable ? 1 : 0),
            (object) ((StardewValley.Object) item).ParentSheetIndex
                    });
                Game1.player.reduceActiveItemByOne();
            }
            else
                Utility.withinRadiusOfPlayer(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y, 3, Game1.player);
        }

        private bool HandleAttemptedSaplingPlacement(StardewValley.Object item, GameLocation location, int x, int y, StardewValley.Farmer who = null)
        {
            Vector2 index1 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));

            Vector2 key2 = new Vector2();
            bool thingsAround = false;
            for (int index2 = x / Game1.tileSize - 2; index2 <= x / Game1.tileSize + 2; ++index2)
            {
                for (int index3 = y / Game1.tileSize - 2; index3 <= y / Game1.tileSize + 2; ++index3)
                {
                    key2.X = (float)index2;
                    key2.Y = (float)index3;
                    if (location.terrainFeatures.ContainsKey(key2) && (location.terrainFeatures[key2] is Tree || location.terrainFeatures[key2] is FruitTree))
                    {
                        //The game would return false here.
                        thingsAround = true;
                        break;
                    }
                }
                if (thingsAround)
                    break;
            }

            if (!thingsAround)
                return false;
            //We only continue if the game already returned.

            if (location.terrainFeatures.ContainsKey(index1))
            {
                if (!(location.terrainFeatures[index1] is HoeDirt) || (location.terrainFeatures[index1] as HoeDirt).crop != null)
                    return false;
                location.terrainFeatures.Remove(index1);
            }
            if (location is Farm && (location.doesTileHaveProperty((int)index1.X, (int)index1.Y, "Diggable", "Back") != null || location.doesTileHavePropertyNoNull((int)index1.X, (int)index1.Y, "Type", "Back").Equals("Grass")) && !location.doesTileHavePropertyNoNull((int)index1.X, (int)index1.Y, "NoSpawn", "Back").Equals("Tree") || location.name.Equals("Greenhouse") && (location.doesTileHaveProperty((int)index1.X, (int)index1.Y, "Diggable", "Back") != null || location.doesTileHavePropertyNoNull((int)index1.X, (int)index1.Y, "Type", "Back").Equals("Stone")))
            {
                Game1.playSound("dirtyHit");
                DelayedAction.playSoundAfterDelay("coin", 100);
                SerializableDictionary<Vector2, TerrainFeature> terrainFeatures = location.terrainFeatures;
                Vector2 key3 = index1;
                FruitTree fruitTree = new FruitTree(item.parentSheetIndex);
                int num12 = location.name.Equals("Greenhouse") ? 1 : 0;
                fruitTree.greenHouseTree = num12 != 0;
                int num13 = location.doesTileHavePropertyNoNull((int)index1.X, (int)index1.Y, "Type", "Back").Equals("Stone") ? 1 : 0;
                fruitTree.greenHouseTileTree = num13 != 0;
                terrainFeatures.Add(key3, (TerrainFeature)fruitTree);
                RemoveTooCloseHudMessage();
                return true;
            }
            RemoveChecks = 3;
            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068"));
            return false;
        }

        /***
         * When the player releases their mouse, remove any too close hud messages.
         ***/
        private void MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if ((e.NewState.LeftButton == ButtonState.Released || e.NewState.RightButton == ButtonState.Released) && (e.PriorState.LeftButton == ButtonState.Pressed || e.PriorState.RightButton == ButtonState.Pressed))
            {
                RemoveTooCloseHudMessage();
            }
        }

        /***
         * For the few ticks following a player trying to put a sapling on an invalid tile when there is another tree nearby, remove any too close hud messages.
         ***/
        private void UpdateTick(object sender, EventArgs e)
        {
            if (RemoveChecks > 0)
            {
                RemoveTooCloseHudMessage();
                RemoveChecks--;
            }
        }

        /***
         * Look through the current hud messages and remove ones that mention that trees are too close together.
         ***/
        private void RemoveTooCloseHudMessage()
        {
            String tooCloseMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060");
            for (int i = 0; i < Game1.hudMessages.Count; i++)
            {
                if (Game1.hudMessages[i].message != null && Game1.hudMessages[i].message.Equals(tooCloseMessage))
                {
                    Game1.hudMessages.RemoveAt(i);
                    i--;
                }
            }
        }

        /***
         * Before a save, check every fruit tree to see if it can grow. If not, make it grow.
         ***/
        private void BeforeSave(object sender, EventArgs e)
        {
            foreach (GameLocation l in Game1.locations)
            {
                foreach (KeyValuePair<Vector2, TerrainFeature> fruitTree in l.terrainFeatures?.Where(item => item.Value is FruitTree))
                {
                    if (!CanFruitTreeGrow(l, fruitTree.Key))
                    {
                        SimulateFruitTreeDayUpdate(l, fruitTree.Value as FruitTree);
                    }
                }
            }
        }

        /***
         * Simulates a day of growth on a fruit tree.
         ***/
        private void SimulateFruitTreeDayUpdate(GameLocation l, FruitTree tree)
        {
            if (tree.daysUntilMature > 28)
                tree.daysUntilMature = 28;

            tree.daysUntilMature--;
            int oldGrowthStage = tree.growthStage;
            tree.growthStage = tree.daysUntilMature > 0 ? (tree.daysUntilMature > 7 ? (tree.daysUntilMature > 14 ? (tree.daysUntilMature > 21 ? 0 : 1) : 2) : 3) : 4;

            //We only want to add a fruit to the tree if our simulated growth caused the tree to fully mature. If it is already mature, the game would have already added a fruit.
            if (oldGrowthStage != 4 && !tree.stump && tree.growthStage == 4 && (tree.struckByLightningCountdown > 0 && !Game1.IsWinter || (Game1.currentSeason.Equals(tree.fruitSeason) || l.name.ToLower().Contains("greenhouse"))))
            {
                tree.fruitsOnTree = Math.Min(3, tree.fruitsOnTree + 1);
                if (l.name.ToLower().Contains("greenhouse"))
                    tree.greenHouseTree = true;
            }
        }

        /***
         * Returns whether a fruit tree at a location tileLocation in a GameLocation l could grow.
         ***/
        private bool CanFruitTreeGrow(GameLocation l, Vector2 tileLocation)
        {
            bool cannotGrow = false;
            foreach (Vector2 surroundingTileLocations in Utility.getSurroundingTileLocationsArray(tileLocation))
            {
                bool flag2 = l.terrainFeatures.ContainsKey(surroundingTileLocations) && l.terrainFeatures[surroundingTileLocations] is HoeDirt && (l.terrainFeatures[surroundingTileLocations] as HoeDirt).crop == null;
                if (l.isTileOccupied(surroundingTileLocations, "") && !flag2)
                {
                    cannotGrow = true;
                    break;
                }
            }

            return !cannotGrow;
        }
    }
}
