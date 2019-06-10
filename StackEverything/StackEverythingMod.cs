using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using StackEverything.ObjectCopiers;
using StackEverything.Patches;
using StackEverything.Patches.Size;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace StackEverything
{
    public class StackEverythingMod : Mod
    {
        public static readonly Type[] PatchedTypes = {typeof(Furniture), typeof(Wallpaper)};
        private readonly ICopier<Furniture> furnitureCopier = new FurnitureCopier();
        private readonly ICopier<SObject> tapperCopier = new TapperCopier();
        private bool isInDecoratableLocation;

        private IList<Furniture> lastKnownFurniture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.lastKnownFurniture = new List<Furniture>();
            HarmonyInstance harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            //This only works if the class' Item.Stack property is not overriden to get {1}, set {}
            //Which means boots, hats, rings, and special items can't be stacked.

            //fix maximumStackSize, getStack, addToStack, and drawInMenu
            IDictionary<string, Type> patchedTypeReplacements = new Dictionary<string, Type>
            {
                [nameof(SObject.maximumStackSize)] = typeof(MaximumStackSizePatch),
                [nameof(SObject.addToStack)] = typeof(AddToStackPatch),
                [nameof(SObject.drawInMenu)] = typeof(DrawInMenuPatch)
            };

            IList<Type> typesToPatch = PatchedTypes.Union(new[] {typeof(SObject)}).ToList();


            if (helper.ModRegistry.IsLoaded("Platonymous.CustomFarming"))
            {
                try
                {
                    typesToPatch.Add(Type.GetType("CustomFarmingRedux.CustomMachine, CustomFarmingRedux"));
                }
                catch (Exception e)
                {
                    this.Monitor.Log("Failed to add support for CFR machines.", LogLevel.Debug);
                    this.Monitor.Log(e.ToString(), LogLevel.Debug);
                }
            }

            foreach (Type t in typesToPatch)
            {
                foreach (KeyValuePair<string, Type> replacement in patchedTypeReplacements)
                {
                    this.Patch(harmony, replacement.Key, t, BindingFlags.Instance | BindingFlags.Public, replacement.Value);
                }
            }

            if (helper.ModRegistry.IsLoaded("Platonymous.CustomFurniture"))
            {
                try
                {
                    this.Patch(harmony, nameof(SObject.drawInMenu), Type.GetType("CustomFurniture.CustomFurniture, CustomFurniture"), BindingFlags.Instance | BindingFlags.Public, typeof(DrawInMenuPatch));
                }
                catch (Exception e)
                {
                    this.Monitor.Log("Failed to add support for Custom Furniture.", LogLevel.Debug);
                    this.Monitor.Log(e.ToString(), LogLevel.Debug);
                }
            }

            //fix furniture pickup in decoratable locations and item placement putting down the whole furniture stack
            IDictionary<string, Tuple<Type, Type>> otherReplacements = new Dictionary<string, Tuple<Type, Type>>()
            {
                {nameof(DecoratableLocation.leftClick), new Tuple<Type, Type>(typeof(DecoratableLocation), typeof(FurniturePickupPatch))},
                {nameof(Utility.tryToPlaceItem), new Tuple<Type, Type>(typeof(Utility), typeof(TryToPlaceItemPatch))},
                {"doDoneFishing", new Tuple<Type, Type>(typeof(FishingRod), typeof(DoDoneFishingPatch))}
            };

            foreach (KeyValuePair<string, Tuple<Type, Type>> replacement in otherReplacements)
            {
                this.Patch(harmony, replacement.Key, replacement.Value.Item1, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, replacement.Value.Item2);
            }

            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        /// <summary>Patches a method with a prefix and/or postfix.</summary>
        /// <param name="harmony">The harmony instance to patch with.</param>
        /// <param name="originalName">The name of the original method to patch.</param>
        /// <param name="originalType">The type of the original method to patch.</param>
        /// <param name="originalSearch">How to search for the original method.</param>
        /// <param name="patchType">The type holding the patches.</param>
        private void Patch(HarmonyInstance harmony, string originalName, Type originalType, BindingFlags originalSearch, Type patchType)
        {
            if (originalType == null)
            {
                throw new ArgumentException("Original type can't be null.");
            }

            if (patchType == null)
            {
                throw new ArgumentException("Patch type can't be null.");
            }

            MethodInfo original = originalType.GetMethods(originalSearch).FirstOrDefault(m => m.Name == originalName);

            if (original == null)
            {
                this.Monitor.Log($"Failed to patch {originalType.Name}::{originalName}: could not find original method.", LogLevel.Error);
                return;
            }

            MethodInfo[] patchMethods = patchType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            MethodInfo prefix = patchMethods.FirstOrDefault(m => m.Name == "Prefix");
            MethodInfo postfix = patchMethods.FirstOrDefault(m => m.Name == "Postfix");

            if (prefix == null && postfix == null)
            {
                this.Monitor.Log($"Failed to patch {originalType.Name}::{originalName}: both prefix and postfix are null.", LogLevel.Error);
            }
            else
            {
                try
                {
                    harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix), postfix == null ? null : new HarmonyMethod(postfix));
                    this.Monitor.Log($"Patched {originalType}::{originalName} with{(prefix == null ? "" : $" {patchType.Name}::{prefix.Name}")}{(postfix == null ? "" : $" {patchType.Name}::{postfix.Name}")}", LogLevel.Trace);
                }
                catch (Exception e)
                {
                    this.Monitor.Log($"Failed to patch {originalType.Name}::{originalName}: {e.Message}", LogLevel.Error);
                }
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Placed down furniture is the same instance as furniture in the inventory, leading to really weird behavior.
            // Instead, we'll copy them.
            if (e.IsMultipleOf(15)) // quarter second
            {
                bool wasInDecoratableLocation = this.isInDecoratableLocation;

                if (!(Game1.currentLocation is DecoratableLocation decLoc))
                {
                    this.isInDecoratableLocation = false;
                    return;
                }

                this.isInDecoratableLocation = true;

                if (wasInDecoratableLocation)
                {
                    for (int i = 0; i < decLoc.furniture.Count; i++)
                    {
                        Furniture f = decLoc.furniture[i];
                        if (!this.lastKnownFurniture.Contains(f) && Game1.player.Items.Contains(f))
                        {
                            Furniture copy = this.furnitureCopier.Copy(f);
                            if (copy != null)
                            {
                                decLoc.furniture[i] = copy;

                                // Custom Furniture sets the location of copied furniture to 0 - this fixes that bug.
                                copy.TileLocation = f.TileLocation;
                                copy.boundingBox.Value = f.boundingBox.Value;
                                copy.defaultBoundingBox.Value = f.defaultBoundingBox.Value;
                                copy.updateDrawPosition();
                            }
                            else
                            {
                                this.Monitor.Log($"Failed to make copy of furniture: {f.Name} - {f.GetType().Name}.", LogLevel.Error);
                            }
                        }
                    }
                }

                this.lastKnownFurniture.Clear();
                foreach (Furniture f in decLoc.furniture)
                {
                    this.lastKnownFurniture.Add(f);
                }
            }
        }

        /// <summary>Raised after objects are added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            // Placed down tappers are the same instance as tappers in the inventory, leading to really weird behavior.
            // Instead, we'll copy them.

            if (e.Location != Game1.player.currentLocation)
            {
                return;
            }

            IDictionary<Vector2, SObject> toReplace = new Dictionary<Vector2, SObject>();
            foreach (KeyValuePair<Vector2, SObject> item in e.Added)
            {
                if (Game1.player.Items.Contains(item.Value))
                {
                    SObject copy = this.tapperCopier.Copy(item.Value);
                    if (copy != null)
                    {
                        toReplace[item.Key] = copy;
                    }
                    else
                    {
                        this.Monitor.Log($"Failed to make copy of an object: {item.Value.Name} - {item.Value.GetType().Name}.", LogLevel.Error);
                    }
                }
            }

            foreach (KeyValuePair<Vector2, SObject> item in toReplace)
            {
                Game1.currentLocation.objects[item.Key] = item.Value;
            }
        }
    }
}