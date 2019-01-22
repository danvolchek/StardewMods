using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace BetterFruitTrees.Patches.JunimoHarvester
{
    /// <summary>
    /// Actually harvest fruit trees.
    /// </summary>
    internal class UpdatePatch
    {
        private static Item lastHarvestedItem;

        public static void Prefix(StardewValley.Characters.JunimoHarvester __instance)
        {
            lastHarvestedItem = Utils.Reflection.GetField<Item>(__instance, "lastItemHarvested").GetValue();
        }

        public static void Postfix(StardewValley.Characters.JunimoHarvester __instance, GameTime time,
            GameLocation location)
        {
            Task backgroundTask = Utils.Reflection.GetField<Task>(__instance, "backgroundTask").GetValue();
            if ((backgroundTask != null && !backgroundTask.IsCompleted) || !Game1.IsMasterGame)
                return;
            if (!Game1.IsMasterGame)
                return;

            int harvestTimer = Utils.GetJunimoHarvesterHarvestTimer(__instance).GetValue();

            harvestTimer += time.ElapsedGameTime.Milliseconds;

            if (harvestTimer <= 0)
                return;

            int newTimer = harvestTimer - time.ElapsedGameTime.Milliseconds;

            if (newTimer > 1000)
                return;

            if (!(harvestTimer >= 1000 && newTimer < 1000))
                return;

            IReflectedField<Item> flastItemHarvested = Utils.Reflection.GetField<Item>(__instance, "lastItemHarvested");

            if (__instance.currentLocation != null && !Utils.Reflection.GetProperty<StardewValley.Buildings.JunimoHut>(__instance, "home").GetValue().noHarvest.Value &&
                Utils.IsAdjacentReadyToHarvestFruitTree(__instance.getTileLocation(), __instance.currentLocation) && (flastItemHarvested.GetValue() == null || flastItemHarvested.GetValue() == lastHarvestedItem))
            {
                Utils.Reflection.GetField<NetEvent1Field<int, NetInt>>(__instance, "netAnimationEvent").GetValue().Fire(5);

                Utils.TryToActuallyHarvestFruitTree(__instance);

                Item lastItemHarvested = flastItemHarvested.GetValue();

                if (lastItemHarvested != null && __instance.currentLocation.farmers.Any())
                {
                    Multiplayer multiplayer =
                        Utils.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
                    multiplayer.broadcastSprites(__instance.currentLocation, new TemporaryAnimatedSprite(
                        "Maps\\springobjects",
                        Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet,
                            lastItemHarvested.ParentSheetIndex, 16, 16), 1000f, 1, 0,
                        __instance.position + new Vector2(0.0f, -40f), false, false,
                        (float)(__instance.getStandingY() / 10000.0 + 0.00999999977648258), 0.02f, Color.White, 4f,
                        -0.01f, 0.0f, 0.0f, false)
                    {
                        motion = new Vector2(0.08f, -0.25f)
                    });
                    if (lastItemHarvested is ColoredObject coloredHarvested)
                        multiplayer.broadcastSprites(__instance.currentLocation, new TemporaryAnimatedSprite(
                            "Maps\\springobjects",
                            Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet,
                                lastItemHarvested.ParentSheetIndex + 1, 16, 16), 1000f, 1, 0,
                            __instance.position + new Vector2(0.0f, -40f), false, false,
                            (float)(__instance.getStandingY() / 10000.0 + 0.0149999996647239), 0.02f,
                            coloredHarvested.color.Value, 4f, -0.01f, 0.0f, 0.0f, false)
                        {
                            motion = new Vector2(0.08f, -0.25f)
                        });
                }
            }
        }
    }
}