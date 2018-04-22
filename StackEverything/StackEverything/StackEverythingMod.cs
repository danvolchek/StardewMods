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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SObject = StardewValley.Object;

namespace StackEverything
{
    public class StackEverythingMod : Mod
    {
        public static Type[] patchedTypes = new Type[] { GetSDVType("Objects.Furniture"), GetSDVType("Objects.Wallpaper") };

        private IList<Furniture> lastKnownFurniture;
        private IList<IObjectCopier> copiers = new List<IObjectCopier>();
        private bool isInDecorateableLocation = false;

        public override void Entry(IModHelper helper)
        {
            lastKnownFurniture = new List<Furniture>();
            var harmony = HarmonyInstance.Create("cat.stackeverything");

            //This only works if the class' Item.Stack property is not overriden to get {1}, set {}
            //Which means boots, hats, rings, and special items can't be stacked.

            //fix maximumStackSize, getStack, addToStack, and drawInMenu
            IDictionary<string, Type> replacements = new Dictionary<string, Type>()
            {
                {"maximumStackSize", typeof(MaximumStackSizePatch) },
                {"getStack", typeof(GetStackPatch) },
                {"addToStack", typeof(AddToStackPatch) },
                {"drawInMenu", typeof(DrawInMenuPatch) }
            };

            foreach (Type t in patchedTypes.Union(new Type[] { GetSDVType("Object") }))
            {
                foreach (KeyValuePair<string, Type> replacement in replacements)
                {
                    MethodInfo original = t.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToList().Find(m => m.Name == replacement.Key);

                    MethodInfo prefix = replacement.Value.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(item => item.Name == "Prefix").FirstOrDefault();
                    MethodInfo postfix = replacement.Value.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(item => item.Name == "Postfix").FirstOrDefault();

                    Monitor.Log($"Patching {original} with {prefix} {postfix}", LogLevel.Trace);

                    harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix), postfix == null ? null : new HarmonyMethod(postfix));
                }
            }

            //fix furniture pickup in decoratable locations
            MethodInfo furniturePrefix = typeof(FurniturePickupPatch).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name == "Prefix");
            MethodInfo originalPickup = typeof(DecoratableLocation).GetMethods(BindingFlags.Instance | BindingFlags.Public).ToList().Find(m => m.Name == "leftClick");

            harmony.Patch(originalPickup, new HarmonyMethod(furniturePrefix), null);

            //add copiers for placed down items
            copiers.Add(new TapperCopier());
            copiers.Add(new FurnitureCopier());

            LocationEvents.LocationObjectsChanged += LocationEvents_LocationObjectsChanged;
            GameEvents.QuarterSecondTick += GameEvents_QuarterSecondTick;
        }

        /// <summary>Placed down furniture is the same instance as furniture in the inventory, leading to really weird behavior. Instead, we'll copy them.</summary>
        private void GameEvents_QuarterSecondTick(object sender, EventArgs e)
        {
            bool wasInDecoratableLocation = isInDecorateableLocation;

            if (!(Game1.currentLocation is DecoratableLocation decLoc))
            {
                isInDecorateableLocation = false;
                return;
            }
            else
            {
                isInDecorateableLocation = true;
            }

            if (wasInDecoratableLocation)
                for (int i = 0; i < decLoc.furniture.Count; i++)
                {
                    Furniture f = decLoc.furniture[i];
                    if (!lastKnownFurniture.Contains(f))
                    {
                        if (Game1.player.items.Contains(f))
                        {
                            Monitor.Log($"{f.GetType().Name} was placed down and exists in the inventory.", LogLevel.Trace);
                            decLoc.furniture[i] = (Furniture)this.Copy(f);
                        }
                    }
                }

            lastKnownFurniture.Clear();
            foreach (Furniture f in decLoc.furniture)
                lastKnownFurniture.Add(f);
        }

        /// <summary>Placed down tappers are the same instance as tappers in the inventory, leading to really weird behavior. Instead, we'll copy them.</summary>
        private void LocationEvents_LocationObjectsChanged(object sender, EventArgsLocationObjectsChanged e)
        {
            IDictionary<Vector2, SObject> toReplace = new Dictionary<Vector2, SObject>();
            foreach (KeyValuePair<Vector2, SObject> item in e.NewObjects)
            {
                if (Game1.player.items.Contains(item.Value))
                {
                    Monitor.Log($"{item.Value.GetType().Name} was placed down and exists in the inventory.", LogLevel.Trace);
                    toReplace[item.Key] = this.Copy(item.Value);
                }
            }

            foreach (KeyValuePair<Vector2, SObject> item in toReplace)
            {
                Game1.currentLocation.objects[item.Key] = item.Value;
            }
        }

        /// <summary>Go through all the copiers and look for one that can do the right copy.</summary>
        private SObject Copy(SObject obj)
        {
            foreach (IObjectCopier copier in copiers)
            {
                if (copier.CanCopy(obj))
                {
                    Monitor.Log($"{obj.GetType().Name} was copied by {copier.GetType().Name}.", LogLevel.Trace);
                    return copier.Copy(obj);
                }
            }
            Monitor.Log($"{obj.GetType().Name} was not copied.", LogLevel.Trace);
            return obj;
        }

        //Big thanks to Routine for this workaround for mac users.
        //https://github.com/Platonymous/Stardew-Valley-Mods/blob/master/PyTK/PyUtils.cs#L117
        /// <summary>Gets the correct type of the object, handling different assembly names for mac/linux users.</summary>
        private static Type GetSDVType(string type)
        {
            string prefix = "StardewValley.";
            Type defaultSDV = Type.GetType(prefix + type + ", Stardew Valley");

            if (defaultSDV != null)
                return defaultSDV;
            else
                return Type.GetType(prefix + type + ", StardewValley");
        }
    }
}