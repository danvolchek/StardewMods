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
        public static readonly Type[] PatchedTypes = { GetSDVType("Objects.Furniture"), GetSDVType("Objects.Wallpaper") };
        private readonly IList<IObjectCopier> copiers = new List<IObjectCopier>();
        private bool isInDecorateableLocation;

        private IList<Furniture> lastKnownFurniture;

        public override void Entry(IModHelper helper)
        {
            this.lastKnownFurniture = new List<Furniture>();
            HarmonyInstance harmony = HarmonyInstance.Create("cat.stackeverything");

            //This only works if the class' Item.Stack property is not overriden to get {1}, set {}
            //Which means boots, hats, rings, and special items can't be stacked.

            //fix maximumStackSize, getStack, addToStack, and drawInMenu
            IDictionary<string, Type> patchedTypeReplacements = new Dictionary<string, Type>
            {
                {"maximumStackSize", typeof(MaximumStackSizePatch)},
                {"addToStack", typeof(AddToStackPatch)},
                {"drawInMenu", typeof(DrawInMenuPatch)}
            };

            IList<Type> typesToPatch = PatchedTypes.Union(new[] { GetSDVType("Object") }).ToList();


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
                foreach (KeyValuePair<string, Type> replacement in patchedTypeReplacements)
                {
                    MethodInfo original = t.GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(m => m.Name == replacement.Key);

                    MethodInfo prefix = replacement.Value
                        .GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Prefix");
                    MethodInfo postfix = replacement.Value
                        .GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Postfix");

                    this.Monitor.Log($"Patching {original} with {prefix} {postfix}", LogLevel.Trace);

                    harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix),
                        postfix == null ? null : new HarmonyMethod(postfix));
                }

            //fix furniture pickup in decoratable locations and item placement putting down the whole furniture stack
            IDictionary<string, Tuple<Type, Type>> otherReplacements = new Dictionary<string, Tuple<Type, Type>>()
            {
                {"leftClick", new Tuple<Type, Type>(GetSDVType("Locations.DecoratableLocation"), typeof(FurniturePickupPatch))},
                {"tryToPlaceItem", new Tuple<Type, Type>(GetSDVType("Utility"), typeof(TryToPlaceItemPatch))},
                {"doDoneFishing", new Tuple<Type, Type>(GetSDVType("Tools.FishingRod"), typeof(DoDoneFishingPatch))}
            };

            foreach (KeyValuePair<string, Tuple<Type, Type>> replacement in otherReplacements)
            {
                MethodInfo original = replacement.Value.Item1
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(m => m.Name == replacement.Key);

                MethodInfo prefix = replacement.Value.Item2
                    .GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Prefix");
                MethodInfo postfix = replacement.Value.Item2
                    .GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Postfix");

                this.Monitor.Log($"Patching {original} with {prefix} {postfix}", LogLevel.Trace);

                harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix),
                    postfix == null ? null : new HarmonyMethod(postfix));
            }

            //add copiers for placed down items
            this.copiers.Add(new TapperCopier());
            this.copiers.Add(new FurnitureCopier());

            LocationEvents.ObjectsChanged += this.LocationEvents_ObjectsChanged;
            GameEvents.QuarterSecondTick += this.GameEvents_QuarterSecondTick;
        }

        /// <summary>
        ///     Placed down furniture is the same instance as furniture in the inventory, leading to really weird behavior.
        ///     Instead, we'll copy them.
        /// </summary>
        private void GameEvents_QuarterSecondTick(object sender, EventArgs e)
        {
            bool wasInDecoratableLocation = this.isInDecorateableLocation;

            if (!(Game1.currentLocation is DecoratableLocation decLoc))
            {
                this.isInDecorateableLocation = false;
                return;
            }

            this.isInDecorateableLocation = true;

            if (wasInDecoratableLocation)
                for (int i = 0; i < decLoc.furniture.Count; i++)
                {
                    Furniture f = decLoc.furniture[i];
                    if (!this.lastKnownFurniture.Contains(f) && Game1.player.Items.Contains(f))
                    {
                        this.Monitor.Log($"{f.GetType().Name} was placed down and exists in the inventory.",
                            LogLevel.Trace);
                        decLoc.furniture[i] = (Furniture)this.Copy(f);
                    }
                }

            this.lastKnownFurniture.Clear();
            foreach (Furniture f in decLoc.furniture) this.lastKnownFurniture.Add(f);
        }

        /// <summary>
        ///     Placed down tappers are the same instance as tappers in the inventory, leading to really weird behavior.
        ///     Instead, we'll copy them.
        /// </summary>
        private void LocationEvents_ObjectsChanged(object sender, EventArgsLocationObjectsChanged e)
        {
            if (e.Location != Game1.player.currentLocation)
                return;

            IDictionary<Vector2, SObject> toReplace = new Dictionary<Vector2, SObject>();
            foreach (KeyValuePair<Vector2, SObject> item in e.Added)
                if (Game1.player.Items.Contains(item.Value))
                {
                    this.Monitor.Log($"{item.Value.GetType().Name} was placed down and exists in the inventory.",
                        LogLevel.Trace);
                    toReplace[item.Key] = this.Copy(item.Value);
                }

            foreach (KeyValuePair<Vector2, SObject> item in toReplace)
                Game1.currentLocation.objects[item.Key] = item.Value;
        }

        /// <summary>Go through all the copiers and look for one that can do the right copy.</summary>
        private SObject Copy(SObject obj)
        {
            foreach (IObjectCopier copier in this.copiers)
                if (copier.CanCopy(obj))
                {
                    this.Monitor.Log($"{obj.GetType().Name} was copied by {copier.GetType().Name}.", LogLevel.Trace);
                    return copier.Copy(obj);
                }

            this.Monitor.Log($"{obj.GetType().Name} was not copied.", LogLevel.Trace);
            return obj;
        }

        //Big thanks to Routine for this workaround for mac users.
        //https://github.com/Platonymous/Stardew-Valley-Mods/blob/master/PyTK/PyUtils.cs#L117
        /// <summary>Gets the correct type of the object, handling different assembly names for mac/linux users.</summary>
        private static Type GetSDVType(string type)
        {
            const string prefix = "StardewValley.";
            Type defaultSDV = Type.GetType(prefix + type + ", Stardew Valley");

            return defaultSDV != null ? defaultSDV : Type.GetType(prefix + type + ", StardewValley");
        }
    }
}