using System;
using System.Collections.Generic;
using System.Linq;


using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile.Tiles;

namespace SprinklerRange
{
    class ModEntry : Mod
    {
        private bool showingSprinklers;
        private bool showingScarecrows;
        private bool showingBeeHouses;

        private Texture2D border;
        private Texture2D beeBorder;

        private ModConfig config;

        private List<Pair<int, int>> uncoveredScarecrowTiles;

        private String activeInventoryItem;

        private static Color SPRINKLER_COLOR = Color.LightSkyBlue;
        private static Color SCARECROW_COLOR = Color.Gray;
        private static Color BOTH_COLOR = Color.Violet;

        private static Color BEEHOUSE_COLOR = Color.Yellow;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            if (config.UseOldColorScheme) {
                SCARECROW_COLOR = Color.IndianRed;
                SPRINKLER_COLOR = Color.DarkGreen;
                BOTH_COLOR = Color.Purple;
            }

            activeInventoryItem = null;

            border = helper.Content.Load<Texture2D>("assets/border.png");
            beeBorder = helper.Content.Load<Texture2D>("assets/bee_border.png");

            uncoveredScarecrowTiles = new List<Pair<int, int>>();

            List<Pair<int, int>> uncovered = new List<Pair<int, int>>
            {
                new Pair<int, int>(-8, -5),
                new Pair<int, int>(-8, -6),
                new Pair<int, int>(-8, -7),
                new Pair<int, int>(-8, -8),
                new Pair<int, int>(-7, -6),
                new Pair<int, int>(-7, -7),
                new Pair<int, int>(-7, -8),
                new Pair<int, int>(-6, -7),
                new Pair<int, int>(-6, -8),
                new Pair<int, int>(-5, -8)
            };

            uncoveredScarecrowTiles.AddRange(uncovered.Select(pair => new Pair<int, int>(pair.Item1, pair.Item2)));
            uncovered.ForEach(tup => tup.Item1 *= -1);
            uncoveredScarecrowTiles.AddRange(uncovered.Select(pair => new Pair<int, int>(pair.Item1, pair.Item2)));
            uncovered.ForEach(tup => tup.Item2 *= -1);
            uncoveredScarecrowTiles.AddRange(uncovered.Select(pair => new Pair<int, int>(pair.Item1, pair.Item2)));
            uncovered.ForEach(tup => tup.Item1 *= -1);
            uncoveredScarecrowTiles.AddRange(uncovered.Select(pair => new Pair<int, int>(pair.Item1, pair.Item2)));

            ControlEvents.KeyPressed += this.KeyPressed;
            GraphicsEvents.OnPreRenderHudEvent += this.RenderHighlights;

            if (config.ShowRangeOfHeldSprinklerOrScarecrowOrBeehouse)
                GameEvents.EighthUpdateTick += this.EighthUpdateTick;

        }

        private void EighthUpdateTick(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            activeInventoryItem = Game1.player.ActiveObject?.name.ToLower();
        }

        private void KeyPressed(object sender, EventArgsKeyPressed e)
        {
            String key = e.KeyPressed.ToString().ToLower();

            if (key == config.SprinklerScarecrowActivationKey.ToLower())
            {
                if (!showingSprinklers && !showingScarecrows)
                {
                    showingSprinklers = true;
                }
                else if (showingSprinklers && showingScarecrows)
                {
                    showingScarecrows = showingSprinklers = false;
                }
                else if (showingSprinklers)
                {
                    showingScarecrows = true;
                    showingSprinklers = false;
                }
                else if (showingScarecrows)
                {
                    showingSprinklers = true;
                }
            }
            else if (key == config.BeeHouseActivationKey.ToLower())
            {
                showingBeeHouses = !showingBeeHouses;
            }
        }

        private void RenderHighlights(object sender, EventArgs e)
        {
            Dictionary<Vector2, Color> positions = new Dictionary<Vector2, Color>();

            List<Vector2> beePositions = new List<Vector2>();

            if (showingScarecrows)
            {
                foreach (StardewValley.Object scarecrow in GetScarecrows())
                {
                    AddScareCrowTiles(positions, scarecrow.getLocalPosition(Game1.viewport));
                }
            }

            if (showingSprinklers)
            {
                foreach (StardewValley.Object sprinkler in GetSprinklers())
                {
                    AddSprinklerTiles(positions, sprinkler.name.ToLower(), sprinkler.getLocalPosition(Game1.viewport));
                }
            }

            if(showingBeeHouses)
            {
                foreach (StardewValley.Object beehouse in GetBeeHouses())
                {
                    AddBeeHouseTiles(beePositions, beehouse.getLocalPosition(Game1.viewport));
                }
            }

            if(activeInventoryItem != null)
            {
                if (activeInventoryItem.Contains("sprinkler"))
                {
                    Vector2 pos = new Vector2(((int)((Game1.getOldMouseX() + Game1.viewport.X) / Game1.tileSize)) * Game1.tileSize - Game1.viewport.X,
                        ((int)((Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize)) * Game1.tileSize - Game1.viewport.Y);

                    AddSprinklerTiles(positions, activeInventoryItem, pos);
                }
                else if (activeInventoryItem.Contains("arecrow"))
                {
                    Vector2 pos = new Vector2(((int)((Game1.getOldMouseX() + Game1.viewport.X) / Game1.tileSize)) * Game1.tileSize - Game1.viewport.X,
                       ((int)((Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize)) * Game1.tileSize - Game1.viewport.Y);

                    AddScareCrowTiles(positions, pos);
                }
                else if (activeInventoryItem.Contains("bee house"))
                {
                    Vector2 pos = new Vector2(((int)((Game1.getOldMouseX() + Game1.viewport.X) / Game1.tileSize)) * Game1.tileSize - Game1.viewport.X,
                       ((int)((Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize)) * Game1.tileSize - Game1.viewport.Y);

                    AddBeeHouseTiles(beePositions, pos);
                }
            }
            

            foreach (KeyValuePair<Vector2, Color> pos in positions)
            {
                Game1.spriteBatch.Draw(border, pos.Key, pos.Value);
            }

            foreach (Vector2 pos in beePositions)
            {
                Game1.spriteBatch.Draw(beeBorder, pos, BEEHOUSE_COLOR);
            }

        }

        private void AddScareCrowTiles(Dictionary<Vector2, Color> positions, Vector2 pos)
        {
            for (var i = -8; i < 9; i++)
                for (var j = -8; j < 9; j++)
                {
                    if (!uncoveredScarecrowTiles.Any(item => item.Item1 == i && item.Item2 == j))
                    {
                        AddIfNotIn(positions, new Vector2(pos.X + i * Game1.tileSize, pos.Y + j * Game1.tileSize), false);
                    }

                }
        }

        private void AddSprinklerTiles(Dictionary<Vector2, Color> positions, String sprinklerName, Vector2 pos)
        {

            AddIfNotIn(positions, pos, true);
            AddIfNotIn(positions, new Vector2(pos.X - Game1.tileSize, pos.Y), true);
            AddIfNotIn(positions, new Vector2(pos.X + Game1.tileSize, pos.Y), true);
            AddIfNotIn(positions, new Vector2(pos.X, pos.Y - Game1.tileSize), true);
            AddIfNotIn(positions, new Vector2(pos.X, pos.Y + Game1.tileSize), true);

            if (sprinklerName.Contains("quality") || sprinklerName.Contains("iridium"))
            {
                AddIfNotIn(positions, new Vector2(pos.X - Game1.tileSize, pos.Y - Game1.tileSize), true);
                AddIfNotIn(positions, new Vector2(pos.X + Game1.tileSize, pos.Y + Game1.tileSize), true);
                AddIfNotIn(positions, new Vector2(pos.X + Game1.tileSize, pos.Y - Game1.tileSize), true);
                AddIfNotIn(positions, new Vector2(pos.X - Game1.tileSize, pos.Y + Game1.tileSize), true);
            }
            if (sprinklerName.Contains("iridium"))
            {
                for (int i = -2; i < 3; i++)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        if (Math.Abs(i) == 2 || Math.Abs(j) == 2)
                        {
                            AddIfNotIn(positions, new Vector2(pos.X + i * Game1.tileSize, pos.Y + j * Game1.tileSize), true);
                        }
                    }
                }
            }
        }

        private void AddBeeHouseTiles(List<Vector2> positions, Vector2 pos)
        {
            for (int i = -5; i < 6; i++)
                for (int j = -4; j < 5; j++)
                    if(Math.Abs(i) + Math.Abs(j) < 6)
                        positions.Add(new Vector2(pos.X + i * Game1.tileSize, pos.Y + j * Game1.tileSize));
        }

        private void AddIfNotIn(Dictionary<Vector2, Color> positions, Vector2 position, bool isSprinkler)
        {
            if (!positions.Keys.Contains(position))
                positions.Add(position, isSprinkler ? SPRINKLER_COLOR : SCARECROW_COLOR);
            else if (positions[position] == (isSprinkler ? SCARECROW_COLOR : SPRINKLER_COLOR))
                positions[position] = BOTH_COLOR;
        }

        private List<StardewValley.Object> GetSprinklers() => GetObject("sprinkler");

        private List<StardewValley.Object> GetScarecrows() => GetObject("arecrow");

        private List<StardewValley.Object> GetBeeHouses() => GetObject("bee house");

        private List<StardewValley.Object> GetObject(string name)
        {
            return Game1.currentLocation.Objects.Where(kvp => kvp.Value.name != null && kvp.Value.name.ToLower().Contains(name)).Select(x => x.Value).ToList();
        }

    }
}
