using System.Linq;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace StackEverything.Patches
{
    internal class DoDoneFishingPatch
    {
        private static SObject tackle;

        public static void Prefix(FishingRod __instance)
        {
            tackle = __instance.attachments?.Count > 1 ? __instance.attachments[1] : null;
        }

        public static void Postfix(FishingRod __instance)
        {
            if (__instance.attachments == null || __instance.attachments?.Count <= 1)
                return;

            if (tackle != null && __instance.attachments[1] == null)
            {
                if (tackle.Stack > 1)
                {
                    tackle.Stack--;
                    tackle.uses.Value = 0;
                    __instance.attachments[1] = tackle;

                    string displayedMessage = new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14086"), "").Message;
                    Game1.hudMessages.Remove(Game1.hudMessages.FirstOrDefault(item =>
                        item.Message == displayedMessage));
                }
            }
        }
    }
}
