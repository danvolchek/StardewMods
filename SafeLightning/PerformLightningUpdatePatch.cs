using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Object = StardewValley.Object;

namespace SafeLightning
{
    /// <summary>Patches <see cref="Utility.performLightningUpdate"/>.</summary>
    [HarmonyPatch]
    internal class PerformLightningUpdatePatch
    {
        /*********
        ** Private methods
        *********/

        /// <summary>The method to be patched.</summary>
        /// <returns><see cref="Utility.performLightningUpdate"/>.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Utility).GetMethod(nameof(Utility.performLightningUpdate));
        }

        /// <summary>The code to run before the original method.</summary>
        /// <returns>Whether to run the original method or not.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prefix()
        {
            PerformLightningUpdatePatch.PerformLightningUpdate();
            return false;
        }

        /// <summary>Performs a lightning update, not harming objects in the world.</summary>
        /// <remarks>Copied from the decompiled vanilla method, with as few changes as possible to make updating easier.</remarks>
        [SuppressMessage("ReSharper", "All", Justification = "Copied from the vanilla decompilation.")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField", Justification = "Copied from the vanilla decompilation.")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast", Justification = "Copied from the vanilla decompilation.")]
        private static void PerformLightningUpdate()
        {
            Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + Game1.timeOfDay);
            if (random.NextDouble() < 0.125 + Game1.player.team.AverageDailyLuck((GameLocation)null) + Game1.player.team.AverageLuckLevel((GameLocation)null) / 100.0)
            {
                Farm.LightningStrikeEvent lightningStrikeEvent = new Farm.LightningStrikeEvent();
                lightningStrikeEvent.bigFlash = true;
                Farm locationFromName = Game1.getLocationFromName("Farm") as Farm;
                List<Vector2> source = new List<Vector2>();
                foreach (KeyValuePair<Vector2, Object> pair in locationFromName.objects.Pairs)
                {
                    if ((bool)((NetFieldBase<bool, NetBool>)pair.Value.bigCraftable) && pair.Value.ParentSheetIndex == 9)
                        source.Add(pair.Key);
                }
                if (source.Count > 0)
                {
                    for (int index1 = 0; index1 < 2; ++index1)
                    {
                        Vector2 index2 = source.ElementAt<Vector2>(random.Next(source.Count));
                        if (locationFromName.objects[index2].heldObject.Value == null)
                        {
                            locationFromName.objects[index2].heldObject.Value = new Object(787, 1, false, -1, 0);
                            locationFromName.objects[index2].minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                            locationFromName.objects[index2].shakeTimer = 1000;
                            lightningStrikeEvent.createBolt = true;
                            lightningStrikeEvent.boltPosition = index2 * 64f + new Vector2(32f, 0.0f);
                            locationFromName.lightningStrikeEvent.Fire(lightningStrikeEvent);
                            return;
                        }
                    }
                }
                if (random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck((GameLocation)null) - Game1.player.team.AverageLuckLevel((GameLocation)null) / 100.0)
                {
                    try
                    {
                        KeyValuePair<Vector2, TerrainFeature> keyValuePair = locationFromName.terrainFeatures.Pairs.ElementAt(random.Next(locationFromName.terrainFeatures.Count()));
                        if (!(keyValuePair.Value is FruitTree))
                        {
                            int num = !(keyValuePair.Value is HoeDirt) || (keyValuePair.Value as HoeDirt).crop == null ? 0 : (!(bool)((NetFieldBase<bool, NetBool>)(keyValuePair.Value as HoeDirt).crop.dead) ? 1 : 0);
                            if (keyValuePair.Value.performToolAction((Tool)null, 50, keyValuePair.Key, (GameLocation)locationFromName))
                            {
                                //lightningStrikeEvent.destroyedTerrainFeature = true;
                                lightningStrikeEvent.createBolt = true;
                                //locationFromName.terrainFeatures.Remove(keyValuePair.Key);
                                lightningStrikeEvent.boltPosition = keyValuePair.Key * 64f + new Vector2(32f, (float)sbyte.MinValue);
                            }
                            if (num != 0)
                            {
                                if (keyValuePair.Value is HoeDirt)
                                {
                                    if ((keyValuePair.Value as HoeDirt).crop != null)
                                    {
                                        if ((bool)((NetFieldBase<bool, NetBool>)(keyValuePair.Value as HoeDirt).crop.dead))
                                        {
                                            lightningStrikeEvent.createBolt = true;
                                            lightningStrikeEvent.boltPosition = keyValuePair.Key * 64f + new Vector2(32f, 0.0f);
                                        }
                                    }
                                }
                            }
                        }
                        else if (keyValuePair.Value is FruitTree)
                        {
                            //(keyValuePair.Value as FruitTree).struckByLightningCountdown.Value = 4;
                            //(keyValuePair.Value as FruitTree).shake(keyValuePair.Key, true, (GameLocation)locationFromName);
                            lightningStrikeEvent.createBolt = true;
                            lightningStrikeEvent.boltPosition = keyValuePair.Key * 64f + new Vector2(32f, (float)sbyte.MinValue);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                locationFromName.lightningStrikeEvent.Fire(lightningStrikeEvent);
            }
            else
            {
                if (random.NextDouble() >= 0.1)
                    return;
                (Game1.getLocationFromName("Farm") as Farm).lightningStrikeEvent.Fire(new Farm.LightningStrikeEvent()
                {
                    smallFlash = true
                });
            }
        }
    }
}
