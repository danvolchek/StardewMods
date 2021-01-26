using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
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
        public static bool Prefix()
        {
            performLightningUpdate();
            return false;
        }

        /// <summary>Performs a lightning update, not harming objects in the world.</summary>
        /// <remarks>Copied from the decompiled vanilla method, with as few changes as possible to make updating easier.</remarks>
        [SuppressMessage("ReSharper", "All", Justification = "Copied from the vanilla decompilation.")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField", Justification = "Copied from the vanilla decompilation.")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast", Justification = "Copied from the vanilla decompilation.")]
        public static void performLightningUpdate()
        {
            Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + Game1.timeOfDay);
            if (random.NextDouble() < 0.125 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() / 100.0)
            {
                Farm.LightningStrikeEvent lightningEvent2 = new Farm.LightningStrikeEvent();
                lightningEvent2.bigFlash = true;
                Farm farm = Game1.getLocationFromName("Farm") as Farm;
                List<Vector2> lightningRods = new List<Vector2>();
                foreach (KeyValuePair<Vector2, Object> v2 in farm.objects.Pairs)
                {
                    if ((bool)v2.Value.bigCraftable && v2.Value.ParentSheetIndex == 9)
                    {
                        lightningRods.Add(v2.Key);
                    }
                }
                if (lightningRods.Count > 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 v = lightningRods.ElementAt(random.Next(lightningRods.Count));
                        if (farm.objects[v].heldObject.Value == null)
                        {
                            farm.objects[v].heldObject.Value = new Object(787, 1);
                            farm.objects[v].minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                            farm.objects[v].shakeTimer = 1000;
                            lightningEvent2.createBolt = true;
                            lightningEvent2.boltPosition = v * 64f + new Vector2(32f, 0f);
                            farm.lightningStrikeEvent.Fire(lightningEvent2);
                            return;
                        }
                    }
                }
                if (random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() - Game1.player.team.AverageLuckLevel() / 100.0)
                {
                    try
                    {
                        KeyValuePair<Vector2, TerrainFeature> c = farm.terrainFeatures.Pairs.ElementAt(random.Next(farm.terrainFeatures.Count()));
                        if (!(c.Value is FruitTree))
                        {
                            bool num = c.Value is HoeDirt && (c.Value as HoeDirt).crop != null && !(c.Value as HoeDirt).crop.dead;
                            if (c.Value.performToolAction(null, 50, c.Key, farm))
                            {
                                //lightningEvent2.destroyedTerrainFeature = true;
                                lightningEvent2.createBolt = true;
                                //farm.terrainFeatures.Remove(c.Key);
                                lightningEvent2.boltPosition = c.Key * 64f + new Vector2(32f, -128f);
                            }
                            if (num && c.Value is HoeDirt && (c.Value as HoeDirt).crop != null && (bool)(c.Value as HoeDirt).crop.dead)
                            {
                                lightningEvent2.createBolt = true;
                                lightningEvent2.boltPosition = c.Key * 64f + new Vector2(32f, 0f);
                            }
                        }
                        else if (c.Value is FruitTree)
                        {
                            //(c.Value as FruitTree).struckByLightningCountdown.Value = 4;
                            //(c.Value as FruitTree).shake(c.Key, doEvenIfStillShaking: true, farm);
                            lightningEvent2.createBolt = true;
                            lightningEvent2.boltPosition = c.Key * 64f + new Vector2(32f, -128f);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                farm.lightningStrikeEvent.Fire(lightningEvent2);
            }
            else if (random.NextDouble() < 0.1)
            {
                Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
                lightningEvent.smallFlash = true;
                Farm farm = Game1.getLocationFromName("Farm") as Farm;
                farm.lightningStrikeEvent.Fire(lightningEvent);
            }
        }
    }
}
