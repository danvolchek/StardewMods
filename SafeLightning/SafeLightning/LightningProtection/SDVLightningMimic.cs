using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace SafeLightning.LightningProtection
{
    /// <summary>
    /// Class for mimicing SDV's lightning code.
    /// </summary>
    internal static class SDVLightningMimic
    {
        /// <summary>
        /// Run an exact copy of SDV's lightning code, using custom RNG paremeters.
        /// </summary>
        /// <param name="info">RNG parameters to use</param>
        internal static void CauseVanillaStrike(LightningStrikeRNGInfo info)
        {
            Random random = info.GetRandom();
            if (random.NextDouble() < 0.125 + info.dailyLuck + info.luckLevel / 100.0)
            {
                if (Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && !Game1.newDay)
                {
                    Game1.flashAlpha = (float)(0.5 + random.NextDouble());
                    Game1.playSound("thunder");
                }
                GameLocation locationFromName = Game1.getLocationFromName("Farm");
                List<Vector2> source = new List<Vector2>();
                foreach (KeyValuePair<Vector2, SObject> keyValuePair in (Dictionary<Vector2, SObject>)locationFromName.objects)
                {
                    if (keyValuePair.Value.bigCraftable && keyValuePair.Value.ParentSheetIndex == 9)
                        source.Add(keyValuePair.Key);
                }
                if (source.Count > 0)
                {
                    for (int index1 = 0; index1 < 2; ++index1)
                    {
                        Vector2 index2 = source.ElementAt<Vector2>(random.Next(source.Count));
                        if (locationFromName.objects[index2].heldObject == null)
                        {
                            locationFromName.objects[index2].heldObject = new SObject(787, 1, false, -1, 0);
                            locationFromName.objects[index2].minutesUntilReady = 3000 - Game1.timeOfDay;
                            locationFromName.objects[index2].shakeTimer = 1000;
                            if (!(Game1.currentLocation is Farm))
                                return;
                            Utility.drawLightningBolt(index2 * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), 0.0f), locationFromName);
                            return;
                        }
                    }
                }
                if (random.NextDouble() >= 0.25 - info.dailyLuck - info.luckLevel / 100.0)
                    return;
                try
                {
                    KeyValuePair<Vector2, TerrainFeature> keyValuePair = locationFromName.terrainFeatures.ElementAt<KeyValuePair<Vector2, TerrainFeature>>(random.Next(locationFromName.terrainFeatures.Count));
                    if (!(keyValuePair.Value is FruitTree) && keyValuePair.Value.performToolAction((Tool)null, 50, keyValuePair.Key, locationFromName))
                    {
                        locationFromName.terrainFeatures.Remove(keyValuePair.Key);
                        if (!Game1.currentLocation.name.Equals("Farm"))
                            return;
                        locationFromName.temporarySprites.Add(new TemporaryAnimatedSprite(362, 75f, 6, 1, keyValuePair.Key, false, false));
                        Utility.drawLightningBolt(keyValuePair.Key * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(-Game1.tileSize * 2)), locationFromName);
                    }
                    else
                    {
                        if (!(keyValuePair.Value is FruitTree))
                            return;
                        (keyValuePair.Value as FruitTree).struckByLightningCountdown = 4;
                        (keyValuePair.Value as FruitTree).shake(keyValuePair.Key, true);
                        Utility.drawLightningBolt(keyValuePair.Key * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(-Game1.tileSize * 2)), locationFromName);
                    }
                }
                catch
                {
                }
            }
            else
            {
                if (random.NextDouble() >= 0.1 || !Game1.currentLocation.IsOutdoors || (Game1.currentLocation is Desert || Game1.newDay))
                    return;
                Game1.flashAlpha = (float)(0.5 + random.NextDouble());
                if (random.NextDouble() < 0.5)
                    DelayedAction.screenFlashAfterDelay((float)(0.3 + random.NextDouble()), random.Next(500, 1000), "");
                DelayedAction.playSoundAfterDelay("thunder_small", random.Next(500, 1500));
            }
        }

        /// <summary>
        /// Gets the non lightning rod <see cref="TerrainFeature"/> that will be hit today under the given RNG info.
        /// </summary>
        /// <param name="when">The time the strike will hit</param>
        /// <param name="feature">The <see cref="TerrainFeature"/> hit, or null if none was hit</param>
        /// <returns>Whether a <see cref="TerrainFeature"/> was hit</returns>
        internal static bool GetSDVLightningStrikePositionAt(LightningStrikeRNGInfo info, out KeyValuePair<Vector2, TerrainFeature>? feature)
        {
            feature = null;
            if (!info.isLightning || Game1.timeOfDay > 2400 || Game1.getFarm().terrainFeatures.Count == 0)
                return false;

            Random random = info.GetRandom();
            if (random.NextDouble() < 0.125 + info.dailyLuck + info.luckLevel / 100.0)
            {
                if (Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && !Game1.newDay)
                {
                    random.NextDouble();
                }

                GameLocation locationFromName = Game1.getLocationFromName("Farm");
                List<Vector2> source = new List<Vector2>();
                foreach (KeyValuePair<Vector2, SObject> keyValuePair in locationFromName.objects)
                {
                    if (keyValuePair.Value.bigCraftable && keyValuePair.Value.ParentSheetIndex == 9)
                        source.Add(keyValuePair.Key);
                }
                if (source.Count > 0)
                {
                    for (int index1 = 0; index1 < 2; ++index1)
                    {
                        Vector2 index2 = source.ElementAt(random.Next(source.Count));
                        if (locationFromName.objects[index2].heldObject == null)
                            return false;
                    }
                }
                if (random.NextDouble() >= 0.25 - info.dailyLuck - info.luckLevel / 100.0)
                    return false;

                feature = locationFromName.terrainFeatures.ElementAt(random.Next(locationFromName.terrainFeatures.Count));

                return true;
            }
            return false;
        }

        /// <summary>
        /// Safely strikes lightning.
        /// </summary>
        /// <param name="position">The position to strike lightning</param>
        /// <param name="effects">Whether visual/sound effects should be created</param>
        internal static void StrikeLightningSafelyAt(IMonitor monitor, Vector2 position, bool effects)
        {
            GameLocation farm = Game1.getFarm();

            if (effects && Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && !Game1.newDay)
            {
                Game1.flashAlpha = (float)(0.5 + new Random().NextDouble());
                Game1.playSound("thunder");
            }

            if (farm.objects.TryGetValue(position, out SObject obj) && obj.bigCraftable && obj.parentSheetIndex == 9 && obj.heldObject == null)
            {
                monitor.Log($"Mod hit lightning rod.", LogLevel.Trace);
                obj.heldObject = new SObject(787, 1, false, -1, 0);
                obj.minutesUntilReady = 3000 - Game1.timeOfDay;
                obj.shakeTimer = 1000;
                if (effects && Game1.currentLocation is Farm)
                {
                    Utility.drawLightningBolt(position * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), 0.0f), farm);
                }
            }
            else
            {
                //Basically the same as SDV's lightning code, but there is control over visual effects,
                //as well as no randomnes in strike position.

                //Copied from the game's code. Needs to be wrapped in a try catch block because some terrain features
                //cause null reference exceptions when Tool is null in feature.performToolAction.
                try
                {
                    if (farm.terrainFeatures.TryGetValue(position, out TerrainFeature feature)
                         && !(feature is FruitTree) && feature.performToolAction(null, 50, position, farm))
                    {
                        monitor.Log($"Mod hit terrain {feature.GetType().Name} at {position}.", LogLevel.Trace);
                        farm.terrainFeatures.Remove(position);
                        if (effects && Game1.currentLocation is Farm)
                        {
                            farm.temporarySprites.Add(new TemporaryAnimatedSprite(362, 75f, 6, 1, position, false, false));
                            Utility.drawLightningBolt(position * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(-Game1.tileSize * 2)), farm);
                        }
                    }
                    else
                    {
                        //Normally this would hit a fruit tree, but we always want to avoid that.
                        if (effects)
                            Utility.drawLightningBolt(position * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(-Game1.tileSize * 2)), farm);
                    }
                }
                catch
                {
                }
            }
        }
    }
}