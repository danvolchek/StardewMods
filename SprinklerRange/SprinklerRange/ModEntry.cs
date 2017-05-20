using System;
using System.Collections.Generic;
using System.Linq;


using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SprinklerRange
{
    class ModEntry : Mod
    {
        private bool showingSprinklers;
        private bool showingScarecrows;

        private Texture2D border;

        private ModConfig config;

        private List<Pair<int, int>> uncoveredScarecrowTiles;

        private static Color SPRINKLER_COLOR = Color.ForestGreen;
        private static Color SCARECROW_COLOR = Color.IndianRed;
        private static Color BOTH_COLOR = Color.Purple;

        public override void Entry(IModHelper helper)
        {
            ControlEvents.KeyPressed += this.KeyPressed;
            GraphicsEvents.OnPreRenderHudEvent += this.RenderHighlights;

            config = helper.ReadConfig<ModConfig>();

            border = helper.Content.Load<Texture2D>("assets/border.png");

            uncoveredScarecrowTiles = new List<Pair<int, int>>();

            List<Pair<int, int>> uncovered = new List<Pair<int, int>>();
            uncovered.Add(new Pair<int, int>(-8, -5));
            uncovered.Add(new Pair<int, int>(-8, -6));
            uncovered.Add(new Pair<int, int>(-8, -7));
            uncovered.Add(new Pair<int, int>(-8, -8));
            uncovered.Add(new Pair<int, int>(-7, -6));
            uncovered.Add(new Pair<int, int>(-7, -7));
            uncovered.Add(new Pair<int, int>(-7, -8));
            uncovered.Add(new Pair<int, int>(-6, -7));
            uncovered.Add(new Pair<int, int>(-6, -8));
            uncovered.Add(new Pair<int, int>(-5, -8));

            uncoveredScarecrowTiles.AddRange(uncovered.Select(pair => new Pair<int, int>(pair.Item1, pair.Item2)));
            uncovered.ForEach(tup => tup.Item1 *= -1);
            uncoveredScarecrowTiles.AddRange(uncovered.Select(pair => new Pair<int, int>(pair.Item1, pair.Item2)));
            uncovered.ForEach(tup => tup.Item2 *= -1);
            uncoveredScarecrowTiles.AddRange(uncovered.Select(pair => new Pair<int, int>(pair.Item1, pair.Item2)));
            uncovered.ForEach(tup => tup.Item1 *= -1);
            uncoveredScarecrowTiles.AddRange(uncovered.Select(pair => new Pair<int, int>(pair.Item1, pair.Item2)));






        }


        private void KeyPressed(object sender, EventArgsKeyPressed e)
        {
            String key = e.KeyPressed.ToString().ToLower();

            if (key == config.ActivationKey.ToLower())
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
        }

        private void RenderHighlights(object sender, EventArgs e)
        {

            Dictionary<Vector2, Color> positions = new Dictionary<Vector2, Color>();

            if (showingScarecrows)
            {
                List<StardewValley.Object> scarecrows = getScarecrows();

                foreach (StardewValley.Object scarecrow in scarecrows)
                {
                    addScareCrowTiles(positions, scarecrow);
                }
            }

            if (showingSprinklers)
            {
                List<StardewValley.Object> sprinklers = getSprinklers();

                foreach (StardewValley.Object sprinkler in sprinklers)
                {
                    addSprinklerTiles(positions, sprinkler);

                }
            }


            foreach (KeyValuePair<Vector2, Color> pos in positions)
            {


                Game1.spriteBatch.Draw(border, pos.Key, pos.Value);
            }



        }

        private void addScareCrowTiles(Dictionary<Vector2, Color> positions, StardewValley.Object scarecrow)
        {
            Vector2 pos = scarecrow.getLocalPosition(Game1.viewport);
            for (var i = -8; i < 9; i++)
                for (var j = -8; j < 9; j++)
                {
                    if (!uncoveredScarecrowTiles.Any(item => item.Item1 == i && item.Item2 == j))
                    {
                        addIfNotIn(positions, new Vector2(pos.X + i * Game1.tileSize, pos.Y + j * Game1.tileSize), false);
                    }

                }

        }


        private void addSprinklerTiles(Dictionary<Vector2, Color> positions, StardewValley.Object sprinkler)
        {
            Vector2 pos = sprinkler.getLocalPosition(Game1.viewport);
            addIfNotIn(positions, pos, true);
            addIfNotIn(positions, new Vector2(pos.X - Game1.tileSize, pos.Y), true);
            addIfNotIn(positions, new Vector2(pos.X + Game1.tileSize, pos.Y), true);
            addIfNotIn(positions, new Vector2(pos.X, pos.Y - Game1.tileSize), true);
            addIfNotIn(positions, new Vector2(pos.X, pos.Y + Game1.tileSize), true);

            if (sprinkler.name.Contains("Quality") || sprinkler.name.Contains("Iridium"))
            {
                addIfNotIn(positions, new Vector2(pos.X - Game1.tileSize, pos.Y - Game1.tileSize), true);
                addIfNotIn(positions, new Vector2(pos.X + Game1.tileSize, pos.Y + Game1.tileSize), true);
                addIfNotIn(positions, new Vector2(pos.X + Game1.tileSize, pos.Y - Game1.tileSize), true);
                addIfNotIn(positions, new Vector2(pos.X - Game1.tileSize, pos.Y + Game1.tileSize), true);
            }
            if (sprinkler.name.Contains("Iridium"))
            {
                for (int i = -2; i < 3; i++)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        if (Math.Abs(i) == 2 || Math.Abs(j) == 2)
                        {
                            addIfNotIn(positions, new Vector2(pos.X + i * Game1.tileSize, pos.Y + j * Game1.tileSize), true);
                        }
                    }
                }
            }
        }

        private void addIfNotIn(Dictionary<Vector2, Color> positions, Vector2 position, bool isSprinkler)
        {
            if (!positions.Keys.Contains(position))
                positions.Add(position, isSprinkler ? Color.ForestGreen : Color.IndianRed);
            else if (positions[position] == (isSprinkler ? Color.IndianRed : Color.ForestGreen))
                positions[position] = Color.Purple;
        }

        private List<StardewValley.Object> getSprinklers()
        {
            return Game1.currentLocation.Objects.Where(kvp => kvp.Value.name != null && kvp.Value.name.ToLower().Contains("sprinkler")).Select(x => x.Value).ToList();
        }

        private List<StardewValley.Object> getScarecrows()
        {
            return Game1.currentLocation.Objects.Where(kvp => kvp.Value.name != null && kvp.Value.name.ToLower().Contains("arecrow")).Select(x => x.Value).ToList();
        }

    }
}
