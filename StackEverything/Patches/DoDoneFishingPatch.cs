using System.Linq;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace StackEverything.Patches
{
    /// <summary>Handle tackle being stacked correctly after it runs out of uses.</summary>
    internal class DoDoneFishingPatch
    {
        private static SObject _tackle;

        public static void Prefix(FishingRod instance)
        {
            _tackle = instance.attachments?.Count > 1 ? instance.attachments[1] : null;
        }

        public static void Postfix(FishingRod instance)
        {
            if (instance.attachments[1] == null || instance.attachments?.Count <= 1)
                return;

            if (_tackle == null || instance.attachments[1] != null) return;
            if (_tackle.Stack <= 1) return;
            _tackle.Stack--;
            _tackle.uses.Value = 0;
            instance.attachments[1] = _tackle;

            var displayedMessage = new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14086"), "").Message;
            Game1.hudMessages.Remove(Game1.hudMessages.FirstOrDefault(item => item.Message == displayedMessage));
        }
    }
}
