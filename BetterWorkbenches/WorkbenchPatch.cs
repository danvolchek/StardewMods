using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace BetterWorkbenches
{
    /// <summary>Patches the workbench to find chests recursively.</summary>
    [HarmonyPatch]
    internal class WorkbenchPatch
    {
        /// <summary>Gets the method to patch.</summary>
        /// <returns>The method to patch.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Workbench).GetMethod(nameof(Workbench.checkForAction));
        }

        /// <summary>The method to run before the original method.</summary>
        /// <returns>Whether to run the original method or not.</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prefix(Farmer who, bool justCheckingForActivity, Workbench __instance, ref bool __result)
        {
            // This shouldn't ever throw an exception, but just in case, run the original method so workbenches can still be used if this mod fails.
            try
            {
                __result = CheckForAction(__instance, who, justCheckingForActivity);
                return false;
            }
            catch
            {
                return true;
            }
        }

        private static Type multipleMutexRequestType;
        private static MethodInfo releaseLocksMethod;

        public static void GetTypes()
        {
            multipleMutexRequestType = Type.GetType("StardewValley.MultipleMutexRequest, StardewValley") ?? Type.GetType("StardewValley.MultipleMutexRequest, Stardew Valley");
            releaseLocksMethod = multipleMutexRequestType.GetMethod("ReleaseLocks");
        }

        private static bool CheckForAction(Workbench workbench, Farmer who, bool justCheckingForActivity)
        {
            if (justCheckingForActivity)
            {
                return true;
            }

            if (workbench.mutex.IsLocked())
            {
                return true;
            }

            List<Chest> nearbyChests = new List<Chest>();
            GetChests(workbench.TileLocation, who, nearbyChests);
            NetMutex[] mutexes = nearbyChests.Select(chest => chest.mutex).ToArray();

            object multipleMutexRequest = null;
            void ReleaseLocks() => releaseLocksMethod.Invoke(multipleMutexRequest, new object[] { });

            multipleMutexRequest = Activator.CreateInstance(multipleMutexRequestType, mutexes, (Action) (() => workbench.mutex.RequestLock((Action) (() =>
            {
                Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
                Game1.activeClickableMenu = (IClickableMenu) new CraftingPage((int) centeringOnScreen.X, (int) centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, false, true, nearbyChests);
                Game1.activeClickableMenu.exitFunction = (IClickableMenu.onExit) (() =>
                {
                    workbench.mutex.ReleaseLock();
                    ReleaseLocks();
                });
            }), ReleaseLocks)), (Action) (() => Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"))));
            return true;
        }

        private static void GetChests(Vector2 position, Farmer who, IList<Chest> nearbyChests)
        {
            // Only check cardinal directions for recursive chests, but all adjacent positions for the workbench (b/c vanilla does this).
            List<Vector2> offsets = new List<Vector2>()
            {
                new Vector2(0.0f, 1f),
                new Vector2(-1f, 0.0f),
                new Vector2(1f, 0.0f),
                new Vector2(0.0f, -1f)
            };

            if (nearbyChests.Count == 0)
            {
                offsets.AddRange(new []{
                    new Vector2(-1f, 1f),
                    new Vector2(1f, 1f),
                    new Vector2(-1f, -1f),
                    new Vector2(1f, -1f)
                });
            }

            foreach (Vector2 offset in offsets)
            {
                Chest chest = null;

                if (who.currentLocation is FarmHouse farmHouse && who.currentLocation.getTileIndexAt((int)(position.X + offset.X), (int)(position.Y + offset.Y), "Buildings") == 173)
                {
                    chest = farmHouse.fridge.Value;
                }
                else
                {
                    Vector2 key = new Vector2((int)(position.X + offset.X), (int)(position.Y + offset.Y));
                    if (who.currentLocation.objects.TryGetValue(key, out StardewValley.Object obj) && obj is Chest foundChest)
                        chest = foundChest;
                }

                if (chest != null && !nearbyChests.Contains(chest))
                {
                    nearbyChests.Add(chest);
                    GetChests(chest.TileLocation, who, nearbyChests);
                }
            }
        }
    }
}
