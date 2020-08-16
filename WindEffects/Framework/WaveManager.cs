using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using WindEffects.Framework.Shakers;

namespace WindEffects.Framework
{
    internal class WaveManager
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private readonly Random rand;
        private readonly ShakerFactory factory = new ShakerFactory();

        private readonly List<Wave> waves = new List<Wave>();
        private double rotation = 0;
        private double waveChance;
        private double gustChance;

        public bool DisableAutoSpawning { get; set; }

        private Vector2[] lastTilesTouched = new Vector2[0];

        public WaveManager(IModHelper helper, IMonitor monitor)
        {
            this.rand = new Random();
            this.helper = helper;
            this.monitor = monitor;
        }

        public void ChangeWavePattern(bool windy)
        {
            this.rotation = rand.NextDouble() * Math.PI * 2;
            this.gustChance = windy ? 0.4 : 0.2;
            this.waveChance = windy ? 0.009 : 0.004; //0.009 is about 42% to see a wave in one second, 0.004 is about 22%
        }

        public void Clear()
        {
            this.waves.Clear();
            this.lastTilesTouched = new Vector2[0];
        }

        public void DebugAdd(Wave wave)
        {
            this.waves.Add(wave);
        }

        public void DebugDraw(SpriteBatch spriteBatch)
        {
            foreach (Wave wave in waves)
                wave.DebugDraw(spriteBatch);
        }


        public void Update(GameLocation location)
        {
            if (!this.DisableAutoSpawning)
                this.spawnWave();

            IDictionary<Vector2, IShaker> tilesTouched = new Dictionary<Vector2, IShaker>();
            IList<Wave> wavesToRemove = new List<Wave>();

            foreach (Wave wave in waves)
            {
                wave.Update();

                Vector2[] waveTilesTouched = wave.TilesTouched();
                if (!waveTilesTouched.Any(tile => location.isTileOnMap(tile)))
                {
                    wavesToRemove.Add(wave);
                }

                foreach (Vector2 tile in waveTilesTouched)
                {
                    if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && factory.TryGetShaker(feature, wave, location, out IShaker shaker))
                    {
                        tilesTouched[tile] = shaker;
                    } else
                    {
                        LargeTerrainFeature largeFeature = location.largeTerrainFeatures.FirstOrDefault(ltf => ltf.tilePosition.Value == tile);
                        if (largeFeature != null && factory.TryGetShaker(largeFeature, wave, location, out shaker))
                        {
                            tilesTouched[tile] = shaker;
                        }
                    }
                }
            }

            foreach (Wave wave in wavesToRemove)
            {
                this.waves.Remove(wave);
            }

            Vector2[] currentTilesTouched = tilesTouched.Keys.Where(key => !lastTilesTouched.Contains(key)).ToArray();

            foreach (Vector2 p in currentTilesTouched)
            {
                tilesTouched[p].Shake(this.helper.Reflection, p);
            }

            this.lastTilesTouched = currentTilesTouched;
        }

        private void spawnWave()
        {
            if (this.waves.Count >= 20)
            {
                this.monitor.Log($"Not spawning a new wave because there are too many, total waves: {this.waves.Count}");
                return;
            }

            if (this.rand.NextDouble() < this.waveChance)
            {
                double amp = rand.NextDouble() * 50 + 10;
                double period = rand.NextDouble() / 100 + 0.005;
                double rotation = this.rotation + rand.NextDouble() * Math.PI / 4;
                double speed = rand.NextDouble() * 5 + 6;

                if (this.rand.NextDouble() < this.gustChance)
                    speed *= 2;

                Tuple<double, double> pos = this.getPosition(rotation);

                Wave wave = new Wave(pos.Item1, pos.Item2, amp, period, rotation, speed);

                if (ModEntry.debug)
                    this.monitor.Log($"New wave at {pos.Item1}, {pos.Item2}, near tile {wave.TilesTouched()[0]}, total waves: {this.waves.Count + 1}");

                this.waves.Add(wave);
            }
        }

        private Tuple<double, double> getPosition(double rotation)
        {
            // Spawn waves just off screen in a way that maximizes their visibility based on their rotation

            /* To do this: Split the screen into 8 zones
             * 
             *  \   1  |  2    /
             *   -------------
             * 8 |           | 3
             * - |  viewport | -
             * 7 |           | 4
             *   -------------
             *  /   6  |  5    \
             * 
             * Go from the center of the viewport opposite the input rotation till exit the screen
             * Find the zone we entered in
             * Choose a random location in that zone
             */

            double minX;
            double minY;
            double xSize;
            double ySize;

            const int bufferSize = Game1.tileSize * 3;

            rotation += Math.PI;

            double centerX = Game1.viewport.X + Game1.viewport.Width / 2;
            double centerY = Game1.viewport.Y + Game1.viewport.Height / 2;

            while (Game1.viewport.Contains(new xTile.Dimensions.Location((int)centerX, (int)centerY)))
            {
                centerX += Game1.tileSize * Math.Cos(rotation);
                centerY += Game1.tileSize * Math.Sin(rotation);
            }

            bool yLow = centerY <= Game1.viewport.Y + Game1.viewport.Height / 2;
            bool xLow = centerX <= Game1.viewport.X + Game1.viewport.Width / 2;

            if (centerX <= Game1.viewport.X)
            {
                if (yLow)
                {
                    // 8
                    minX = Game1.viewport.X - bufferSize;
                    minY = Game1.viewport.Y - bufferSize;
                    xSize = bufferSize;
                    ySize = bufferSize + Game1.viewport.Height / 2;
                }
                else
                {
                    // 7
                    minX = Game1.viewport.X - bufferSize;
                    minY = Game1.viewport.Y + Game1.viewport.Height / 2;
                    xSize = bufferSize;
                    ySize = bufferSize + Game1.viewport.Height / 2;
                }
            }
            else if (centerX >= Game1.viewport.X + Game1.viewport.Width)
            {
                if (yLow)
                {
                    // 3
                    minX = Game1.viewport.X + Game1.viewport.Width;
                    minY = Game1.viewport.Y - bufferSize;
                    xSize = bufferSize;
                    ySize = bufferSize + Game1.viewport.Height / 2;
                }
                else
                {
                    // 4
                    minX = Game1.viewport.X + Game1.viewport.Width;
                    minY = Game1.viewport.Y + Game1.viewport.Height / 2;
                    xSize = bufferSize;
                    ySize = bufferSize + Game1.viewport.Height / 2;
                }
            }
            else if (centerY <= Game1.viewport.Y)
            {
                if (xLow)
                {
                    // 1
                    minX = Game1.viewport.X - bufferSize;
                    minY = Game1.viewport.Y - bufferSize;
                    xSize = bufferSize + Game1.viewport.Width / 2;
                    ySize = bufferSize;
                }
                else
                {
                    // 2
                    minX = Game1.viewport.X + Game1.viewport.Width / 2;
                    minY = Game1.viewport.Y - bufferSize;
                    xSize = bufferSize + Game1.viewport.Width / 2;
                    ySize = bufferSize;
                }
            }
            else
            {
                if (xLow)
                {
                    // 6
                    minX = Game1.viewport.X - bufferSize;
                    minY = Game1.viewport.Y + Game1.viewport.Height;
                    xSize = bufferSize + Game1.viewport.Width / 2;
                    ySize = bufferSize;
                }
                else
                {
                    // 5
                    minX = Game1.viewport.X + Game1.viewport.Width / 2;
                    minY = Game1.viewport.Y + Game1.viewport.Height;
                    xSize = bufferSize + Game1.viewport.Width / 2;
                    ySize = bufferSize;
                }
            }

            return new Tuple<double, double>(minX + this.rand.NextDouble() * xSize, minY + this.rand.NextDouble() * ySize);
        }

    }
}
