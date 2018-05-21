using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace BetterFruitTrees
{
    /// <summary>
    ///     Helps the player put down saplings where they want.
    /// </summary>
    internal class PlacementHelper : IInitializable
    {
        private int removeChecks;

        /// <summary>
        ///     Initialize event handlers.
        /// </summary>
        public void Init()
        {
            SaveEvents.BeforeSave += this.BeforeSave;
            InputEvents.ButtonPressed += this.ButtonPressed;
            GameEvents.UpdateTick += this.UpdateTick;
            ControlEvents.MouseChanged += this.MouseChanged;
        }

        /// <summary>
        ///     When a button is pressed, if the player was trying to place down a sapling but couldn't due to other trees, place
        ///     down a sapling.
        /// </summary>
        private void ButtonPressed(object sender, EventArgsInput e)
        {
            if ((e.IsActionButton || e.IsUseToolButton) && Game1.player.ActiveObject != null &&
                Game1.player.ActiveObject.Name.Contains("Sapling"))
            {
                Vector2 vector2 = this.GetMouseTile();

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
                        this.TryToPlaceSapling(Game1.currentLocation, Game1.player.ActiveObject,
                            (int) vector2.X * Game1.tileSize + Game1.tileSize / 2,
                            (int) vector2.Y * Game1.tileSize + Game1.tileSize / 2);
                }
            }
        }

        /// <summary>
        ///     Gets the tile the mouse cursor is at.
        /// </summary>
        private Vector2 GetMouseTile()
        {
            Vector2 vector2 =
                new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) /
                Game1.tileSize;
            if (Game1.mouseCursorTransparency == 0.0 || !Game1.wasMouseVisibleThisFrame ||
                !Game1.lastCursorMotionWasMouse && (Game1.player.ActiveObject == null ||
                                                    !Game1.player.ActiveObject.isPlaceable() &&
                                                    Game1.player.ActiveObject.Category != -74))
            {
                vector2 = Game1.player.GetGrabTile();
                if (vector2.Equals(Game1.player.getTileLocation()))
                    vector2 = Utility.getTranslatedVector2(vector2, Game1.player.facingDirection, 1f);
            }

            if (!Utility.tileWithinRadiusOfPlayer((int) vector2.X, (int) vector2.Y, 1, Game1.player))
            {
                vector2 = Game1.player.GetGrabTile();
                if (vector2.Equals(Game1.player.getTileLocation()) && Game1.isAnyGamePadButtonBeingPressed())
                    vector2 = Utility.getTranslatedVector2(vector2, Game1.player.facingDirection, 1f);
            }

            return vector2;
        }

        /// <summary>
        ///     Try to place a sapling at the given position.
        /// </summary>
        private void TryToPlaceSapling(GameLocation location, Item item, int x, int y)
        {
            if (item is Tool)
                return;

            if (Utility.playerCanPlaceItemHere(location, item, x, y, Game1.player))
            {
                if (!this.PlaceSaplingRegardlessOfNearbyTrees((Object) item, location, x, y, Game1.player))
                    return;

                Game1.player.reduceActiveItemByOne();
            }
            else
            {
                Utility.withinRadiusOfPlayer(Game1.getOldMouseX() + Game1.viewport.X,
                    Game1.getOldMouseY() + Game1.viewport.Y, 3, Game1.player);
            }
        }

        /// <summary>
        ///     Place a sapling regardless of other trees.
        /// </summary>
        /// <returns>Whether placement was successful.</returns>
        private bool PlaceSaplingRegardlessOfNearbyTrees(Object item, GameLocation location, int x, int y,
            Farmer who = null)
        {
            Vector2 index1 = new Vector2(x / Game1.tileSize, y / Game1.tileSize);

            Vector2 key2 = new Vector2();
            bool thingsAround = false;
            for (int index2 = x / Game1.tileSize - 2; index2 <= x / Game1.tileSize + 2; ++index2)
            {
                for (int index3 = y / Game1.tileSize - 2; index3 <= y / Game1.tileSize + 2; ++index3)
                {
                    key2.X = index2;
                    key2.Y = index3;
                    if (location.terrainFeatures.ContainsKey(key2) &&
                        (location.terrainFeatures[key2] is Tree || location.terrainFeatures[key2] is FruitTree))
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
                if (!(location.terrainFeatures[index1] is HoeDirt) ||
                    (location.terrainFeatures[index1] as HoeDirt).crop != null)
                    return false;
                location.terrainFeatures.Remove(index1);
            }

            if (location is Farm &&
                (location.doesTileHaveProperty((int) index1.X, (int) index1.Y, "Diggable", "Back") != null || location
                     .doesTileHavePropertyNoNull((int) index1.X, (int) index1.Y, "Type", "Back").Equals("Grass")) &&
                !location.doesTileHavePropertyNoNull((int) index1.X, (int) index1.Y, "NoSpawn", "Back")
                    .Equals("Tree") || location.Name.Equals("Greenhouse") &&
                (location.doesTileHaveProperty((int) index1.X, (int) index1.Y, "Diggable", "Back") != null || location
                     .doesTileHavePropertyNoNull((int) index1.X, (int) index1.Y, "Type", "Back").Equals("Stone")))
            {
                Game1.playSound("dirtyHit");
                DelayedAction.playSoundAfterDelay("coin", 100);
                location.terrainFeatures.Add(index1, new FruitTree(item.ParentSheetIndex)
                {
                    GreenHouseTree = location.IsGreenhouse,
                    GreenHouseTileTree =
                        location.doesTileHavePropertyNoNull((int) index1.X, (int) index1.Y, "Type", "Back")
                            .Equals("Stone")
                });
                this.RemoveTooCloseHudMessage();
                return true;
            }

            this.removeChecks = 3;
            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068"));
            return false;
        }

        /// <summary>
        ///     When the player releases their mouse, remove any too close hud messages.
        /// </summary>
        private void MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if ((e.NewState.LeftButton == ButtonState.Released || e.NewState.RightButton == ButtonState.Released) &&
                (e.PriorState.LeftButton == ButtonState.Pressed || e.PriorState.RightButton == ButtonState.Pressed))
                this.RemoveTooCloseHudMessage();
        }

        /// <summary>
        ///     For the few ticks following a player trying to put a sapling on an invalid tile when there is another tree nearby,
        ///     remove any too close hud messages.
        /// </summary>
        private void UpdateTick(object sender, EventArgs e)
        {
            if (this.removeChecks > 0)
            {
                this.RemoveTooCloseHudMessage();
                this.removeChecks--;
            }
        }

        /// <summary>
        ///     Look through the current hud messages and remove ones that mention that trees are too close together.
        /// </summary>
        private void RemoveTooCloseHudMessage()
        {
            string tooCloseMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060");
            for (int i = 0; i < Game1.hudMessages.Count; i++)
                if (Game1.hudMessages[i].message != null && Game1.hudMessages[i].message.Equals(tooCloseMessage))
                {
                    Game1.hudMessages.RemoveAt(i);
                    i--;
                }
        }

        /// <summary>
        ///     Before a save, check every fruit tree to see if it can grow. If not, make it grow.
        /// </summary>
        private void BeforeSave(object sender, EventArgs e)
        {
            foreach (GameLocation l in Game1.locations)
            foreach (KeyValuePair<Vector2, NetRef<TerrainFeature>> fruitTree in l.terrainFeatures.FieldPairs.Where(
                item =>
                    item.Value.Value is FruitTree))
                if (!this.CanFruitTreeGrow(l, fruitTree.Key))
                    this.SimulateFruitTreeDayUpdate(l, fruitTree.Value.Value as FruitTree);
        }

        /// <summary>
        ///     Simulates a day of growth on a fruit tree.
        /// </summary>
        private void SimulateFruitTreeDayUpdate(GameLocation l, FruitTree tree)
        {
            if (tree.daysUntilMature.Value > 28)
                tree.daysUntilMature.Value = 28;
            tree.daysUntilMature.Value--;
            int oldGrowthStage = tree.growthStage.Value;
            tree.growthStage.Value = tree.daysUntilMature.Value > 0
                ? (tree.daysUntilMature.Value > 7
                    ? (tree.daysUntilMature.Value > 14 ? (tree.daysUntilMature.Value > 21 ? 0 : 1) : 2)
                    : 3)
                : 4;

            //We only want to add a fruit to the tree if our simulated growth caused the tree to fully mature. If it is already mature, the game would have already added a fruit.
            if (oldGrowthStage != 4 && !tree.stump.Value && tree.growthStage.Value == 4 &&
                (tree.struckByLightningCountdown.Value > 0 && !Game1.IsWinter ||
                 Game1.currentSeason.Equals(tree.fruitSeason.Value) || l.Name.ToLower().Contains("greenhouse")))
            {
                tree.fruitsOnTree.Value = Math.Min(3, tree.fruitsOnTree.Value + 1);
                if (l.Name.ToLower().Contains("greenhouse"))
                    tree.GreenHouseTree = true;
            }
        }

        /// <summary>
        ///     Whether a fruit tree at the given tile and game location could grow.
        /// </summary>
        private bool CanFruitTreeGrow(GameLocation l, Vector2 tileLocation)
        {
            bool cannotGrow = false;
            foreach (Vector2 surroundingTileLocations in Utility.getSurroundingTileLocationsArray(tileLocation))
            {
                bool flag2 = l.terrainFeatures.ContainsKey(surroundingTileLocations) &&
                             l.terrainFeatures[surroundingTileLocations] is HoeDirt &&
                             (l.terrainFeatures[surroundingTileLocations] as HoeDirt).crop == null;
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