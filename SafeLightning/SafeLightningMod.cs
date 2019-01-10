using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using SafeLightning.API;
using SafeLightning.CommandParsing;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Reflection;
using StardewValley.Locations;
using SObject = StardewValley.Object;

namespace SafeLightning
{
    /// <summary>
    ///     A mod which prevents negative affects from lightning hitting anything but lightning rods.
    /// </summary>
    public class SafeLightningMod : Mod
    {
        /// <summary>
        ///     Delegate for the method the API will call when it is notified about a new strike.
        /// </summary>
        /// <param name="position">The position to be hit</param>
        /// <param name="effects">Whether to display visual/sound effects or not</param>
        public delegate void StrikeLightningDelegate(Vector2 position, bool effects);


        public static SafeLightningMod Instance;

        /// <summary>
        ///     Safe Lightning initialization. Sets up console commands, lightning detectors and resolvers, and subscribes to
        ///     events.
        /// </summary>
        /// <param name="helper">The mod helper</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this;
            new CommandParser(this);

            HarmonyInstance instance = HarmonyInstance.Create("cat.safelightning");

            instance.PatchAll(Assembly.GetExecutingAssembly());
        }


        /*********
         * Mod API
         *********/

        /// <summary>
        ///     Returns a new instance of the API.
        /// </summary>
        /// <returns>This mod's api</returns>
        public override object GetApi()
        {
            return new SafeLightningApi(this.ModWantsToStrikeLightningAt);
        }

        /// <summary>
        ///     When told that another mod will cause a lightning strike, save the state of that <see cref="TerrainFeature" /> so
        ///     it can be restored.
        /// </summary>
        /// <param name="position">The position that will be hit</param>
        /// <param name="effects">Whether visual/sound effects should be created</param>
        private void ModWantsToStrikeLightningAt(Vector2 position, bool effects)
        {
            if (!Context.IsWorldReady)
                return;

            StrikeLightningSafelyAt(position, effects);
        }

        internal static void StrikeLightningSafely()
        {
            Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + Game1.timeOfDay);

            if (random.NextDouble() < 0.125 + Game1.dailyLuck + Game1.player.luckLevel.Value / 100.0)
            {
                if (Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && !Game1.newDay)
                {

                    Game1.flashAlpha = (float)(0.5 + random.NextDouble());
                    Game1.playSound("thunder");
                }

                GameLocation locationFromName = Game1.getLocationFromName("Farm");
                List<Vector2> source = new List<Vector2>();
                foreach (KeyValuePair<Vector2, SObject> pair in locationFromName.objects.Pairs)
                {
                    if (pair.Value.bigCraftable.Value && pair.Value.ParentSheetIndex == 9)
                        source.Add(pair.Key);
                }

                if (source.Count > 0)
                {
                    for (int index1 = 0; index1 < 2; ++index1)
                    {
                        Vector2 index2 = source.ElementAt<Vector2>(random.Next(source.Count));
                        if (locationFromName.objects[index2].heldObject.Value == null)
                        {
                            locationFromName.objects[index2].heldObject.Value = new SObject(787, 1, false, -1, 0);
                            locationFromName.objects[index2].MinutesUntilReady = 3000 - Game1.timeOfDay;
                            locationFromName.objects[index2].shakeTimer = 1000;
                            if (!(Game1.currentLocation is Farm))
                                return;

                            Utility.drawLightningBolt(index2 * 64f + new Vector2(32f, 0.0f), locationFromName);
                            return;
                        }
                    }
                }

                if (random.NextDouble() >= 0.25 - Game1.dailyLuck - Game1.player.luckLevel.Value / 100.0)
                    return;

                Vector2 pos = locationFromName.terrainFeatures.Pairs.ElementAt(random.Next(locationFromName.terrainFeatures.Count())).Key;
                Utility.drawLightningBolt(pos * 64f + new Vector2(32f, (float)sbyte.MinValue), locationFromName);

            }
            else
            {
                if (random.NextDouble() >= 0.1 || !Game1.currentLocation.IsOutdoors || (Game1.currentLocation is Desert || Game1.newDay))
                    return;

                Game1.flashAlpha = (float)(0.5 + random.NextDouble());
                if (random.NextDouble() < 0.5)
                    DelayedAction.screenFlashAfterDelay((float)(0.3 + random.NextDouble()), random.Next(500, 1000), "");
                DelayedAction.playSoundAfterDelay("thunder_small", random.Next(500, 1500), (GameLocation)null);
            }
        }

        /// <summary>
        ///     Safely strikes lightning.
        /// </summary>
        /// <param name="position">The position to strike lightning</param>
        /// <param name="effects">Whether visual/sound effects should be created</param>
        internal static void StrikeLightningSafelyAt(Vector2 position, bool effects)
        {
            GameLocation farm = Game1.getFarm();

            if (effects && Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert) && !Game1.newDay)
            {
                Game1.flashAlpha = (float)(0.5 + new Random().NextDouble());
                Game1.playSound("thunder");
            }

            if (farm.objects.TryGetValue(position, out SObject obj) && obj.bigCraftable.Value && obj.ParentSheetIndex == 9 &&
                obj.heldObject.Value == null)
            {
                obj.heldObject.Value = new SObject(787, 1, false, -1, 0);
                obj.MinutesUntilReady = 3000 - Game1.timeOfDay;
                obj.shakeTimer = 1000;
                if (effects && Game1.currentLocation is Farm)
                    Utility.drawLightningBolt(position * Game1.tileSize + new Vector2(Game1.tileSize / 2, 0.0f), farm);
            }
            else
            {
                if (effects)
                    Utility.drawLightningBolt(position * Game1.tileSize + new Vector2(Game1.tileSize / 2, -Game1.tileSize * 2), farm);
            }
        }
    }
}