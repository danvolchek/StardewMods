using StardewValley;
using StardewValley.Characters;

namespace RemovableHorseHats
{
    internal class HorseCheckActionPatch
    {
        public static bool Prefix(Horse __instance, ref bool __result, Farmer who, GameLocation l)
        {
            if (__instance.rider != null)
                return true;
            if (__instance.Name.Length <= 0 && Game1.player.horseName.Value == null)
                return true;
            if (!RemovableHorseHatsMod.IsRemoveHatKeyDown)
                return true;

            if (Game1.player.addItemToInventory(__instance.hat.Value) != null)
                Game1.createItemDebris(__instance.hat.Value, __instance.position, __instance.facingDirection);

            __instance.hat.Value = null;
            Game1.playSound("dirtyHit");
            __result = true;
            return false;
        }
    }
}