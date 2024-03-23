using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Extensions;
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
        private static bool Prefix(int time_of_day)
        {
            PerformLightningUpdatePatch.PerformLightningUpdate(time_of_day);
            return false;
        }

        /// <summary>Performs a lightning update, not harming objects in the world.</summary>
        /// <remarks>Copied from the decompiled vanilla method, with as few changes as possible to make updating easier.</remarks>
        [SuppressMessage("ReSharper", "All", Justification = "Copied from the vanilla decompilation.")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidNetField", Justification = "Copied from the vanilla decompilation.")]
        [SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast", Justification = "Copied from the vanilla decompilation.")]
        private static void PerformLightningUpdate(int time_of_day)
        {
            Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, time_of_day);
            if (random.NextDouble() < 0.125 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() / 100.0)
            {
                Farm.LightningStrikeEvent lightningStrikeEvent = new Farm.LightningStrikeEvent();
                lightningStrikeEvent.bigFlash = true;
                Farm farm = Game1.getFarm();
                List<Vector2> list = new List<Vector2>();
                foreach (KeyValuePair<Vector2, Object> pair in farm.objects.Pairs)
                {
                    if (pair.Value.QualifiedItemId == "(BC)9")
                    {
                        list.Add(pair.Key);
                    }
                }

                if (list.Count > 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 vector = random.ChooseFrom(list);
                        if (farm.objects[vector].heldObject.Value == null)
                        {
                            farm.objects[vector].heldObject.Value = ItemRegistry.Create<Object>("(O)787");
                            farm.objects[vector].minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                            farm.objects[vector].shakeTimer = 1000;
                            lightningStrikeEvent.createBolt = true;
                            lightningStrikeEvent.boltPosition = vector * 64f + new Vector2(32f, 0f);
                            farm.lightningStrikeEvent.Fire(lightningStrikeEvent);
                            return;
                        }
                    }
                }

                if (random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() - Game1.player.team.AverageLuckLevel() / 100.0)
                {
                    try
                    {
                        if (Utility.TryGetRandom(farm.terrainFeatures, out var key, out var value))
                        {
                            FruitTree fruitTree = value as FruitTree;
                            if (fruitTree != null)
                            {
                                //fruitTree.struckByLightningCountdown.Value = 4;
                                //fruitTree.shake(key, doEvenIfStillShaking: true);
                                lightningStrikeEvent.createBolt = true;
                                lightningStrikeEvent.boltPosition = key * 64f + new Vector2(32f, -128f);
                            }
                            else
                            {
                                Crop crop = (value as HoeDirt)?.crop;
                                bool num = crop != null && !crop.dead;
                                if (value.performToolAction(null, 50, key))
                                {
                                    //lightningStrikeEvent.destroyedTerrainFeature = true;
                                    lightningStrikeEvent.createBolt = true;
                                    //farm.terrainFeatures.Remove(key);
                                    lightningStrikeEvent.boltPosition = key * 64f + new Vector2(32f, -128f);
                                }

                                if (num && (bool)crop.dead)
                                {
                                    lightningStrikeEvent.createBolt = true;
                                    lightningStrikeEvent.boltPosition = key * 64f + new Vector2(32f, 0f);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                farm.lightningStrikeEvent.Fire(lightningStrikeEvent);
            }
            else if (random.NextDouble() < 0.1)
            {
                Farm.LightningStrikeEvent lightningStrikeEvent2 = new Farm.LightningStrikeEvent();
                lightningStrikeEvent2.smallFlash = true;
                Farm farm = Game1.getFarm();
                farm.lightningStrikeEvent.Fire(lightningStrikeEvent2);
            }
        }
    }
}
