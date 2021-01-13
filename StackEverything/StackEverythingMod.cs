using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
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
        public static readonly Type[] PatchedTypes = { typeof(Furniture), typeof(Wallpaper) };
        private readonly ICopier<Furniture> _furnitureCopier = new FurnitureCopier();
        private bool _isInDecoratableLocation;

        private IList<Furniture> _lastKnownFurniture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            _lastKnownFurniture = new List<Furniture>();
            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            //This only works if the class' Item.Stack property is not overriden to get {1}, set {}
            //Which means boots, hats, rings, and special items can't be stacked.

            //fix maximumStackSize, getStack, addToStack, and drawInMenu
            IDictionary<string, Type> patchedTypeReplacements = new Dictionary<string, Type>
            {
                [nameof(SObject.maximumStackSize)] = typeof(MaximumStackSizePatch),
                [nameof(SObject.addToStack)] = typeof(AddToStackPatch),
                [nameof(SObject.drawInMenu)] = typeof(DrawInMenuPatch)
            };

            IList<Type> typesToPatch = PatchedTypes.ToList();

            foreach (var t in typesToPatch)
            {
                foreach (var replacement in patchedTypeReplacements)
                {
                    Patch(harmony, replacement.Key, t, BindingFlags.Instance | BindingFlags.Public, replacement.Value);
                }
            }

            if (helper.ModRegistry.IsLoaded("Platonymous.CustomFurniture"))
            {
                try
                {
                    Patch(harmony, nameof(SObject.drawInMenu), Type.GetType("CustomFurniture.CustomFurniture, CustomFurniture"), BindingFlags.Instance | BindingFlags.Public, typeof(DrawInMenuPatch));
                }
                catch (Exception e)
                {
                    Monitor.Log("Failed to add support for Custom Furniture.");
                    Monitor.Log(e.ToString());
                }
            }

            //fix furniture pickup in decoratable locations and item placement putting down the whole furniture stack
            IDictionary<string, Tuple<Type, Type>> otherReplacements = new Dictionary<string, Tuple<Type, Type>>
            {
                {"removeQueuedFurniture", new Tuple<Type, Type>(typeof(DecoratableLocation), typeof(RemoveQueuedFurniturePatch))},
                {nameof(Utility.tryToPlaceItem), new Tuple<Type, Type>(typeof(Utility), typeof(TryToPlaceItemPatch))},
                {"doDoneFishing", new Tuple<Type, Type>(typeof(FishingRod), typeof(DoDoneFishingPatch))},
                {nameof(Item.canStackWith), new Tuple<Type, Type>(typeof(Item), typeof(CanStackWithPatch))}
            };

            foreach (var replacement in otherReplacements)
            {
                Patch(harmony, replacement.Key, replacement.Value.Item1, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, replacement.Value.Item2);
            }

            // Make tackle stack
            Patch(harmony, nameof(SObject.maximumStackSize), typeof(SObject), BindingFlags.Instance | BindingFlags.Public, typeof(MaximumStackSizePatch));

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
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

            var original = originalType.GetMethods(originalSearch).FirstOrDefault(m => m.Name == originalName);

            if (original == null)
            {
                Monitor.Log($"Failed to patch {originalType.Name}::{originalName}: could not find original method.", LogLevel.Error);
                return;
            }

            var patchMethods = patchType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            var prefix = patchMethods.FirstOrDefault(m => m.Name == "Prefix");
            var postfix = patchMethods.FirstOrDefault(m => m.Name == "Postfix");

            if (prefix == null && postfix == null)
            {
                Monitor.Log($"Failed to patch {originalType.Name}::{originalName}: both prefix and postfix are null.", LogLevel.Error);
            }
            else
            {
                try
                {
                    harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix), postfix == null ? null : new HarmonyMethod(postfix));
                    Monitor.Log($"Patched {originalType}::{originalName} with{(prefix == null ? "" : $" {patchType.Name}::{prefix.Name}")}{(postfix == null ? "" : $" {patchType.Name}::{postfix.Name}")}");
                }
                catch (Exception e)
                {
                    Monitor.Log($"Failed to patch {originalType.Name}::{originalName}: {e.Message}", LogLevel.Error);
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
            if (!e.IsMultipleOf(15)) return;
            var wasInDecoratableLocation = _isInDecoratableLocation;

            if (!(Game1.currentLocation is DecoratableLocation decLoc))
            {
                _isInDecoratableLocation = false;
                return;
            }

            _isInDecoratableLocation = true;

            if (wasInDecoratableLocation)
            {
                for (var i = 0; i < decLoc.furniture.Count; i++)
                {
                    var f = decLoc.furniture[i];
                    if (_lastKnownFurniture.Contains(f) || !Game1.player.Items.Contains(f)) continue;
                    Monitor.Log("Found a chair both in the world and in the inventory!", LogLevel.Error);
                    var copy = _furnitureCopier.Copy(f);
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
                        Monitor.Log($"Failed to make copy of furniture: {f.Name} - {f.GetType().Name}.", LogLevel.Error);
                    }
                }
            }

            _lastKnownFurniture.Clear();
            foreach (var f in decLoc.furniture)
            {
                _lastKnownFurniture.Add(f);
            }
        }
    }
}
