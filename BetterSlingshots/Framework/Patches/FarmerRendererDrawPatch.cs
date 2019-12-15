using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Tools;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BetterSlingshots.Framework.Patches
{
    /// <summary>Draws the farmer's arms in the right spot if reverse aiming is disabled.</summary>
    [HarmonyPatch]
    internal class FarmerRendererDrawPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prepare()
        {
            return BetterSlingshotsMod.Instance.Config.DisableReverseAiming;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(FarmerRenderer).GetMethod(nameof(FarmerRenderer.draw),
                new[]{typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame),
                            typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2),
                            typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer)});
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static void Prefix(Farmer who)
        {
            if (!who.usingSlingshot || !who.IsLocalPlayer)
            {
                return;
            }

            NetPoint aimPos = (who.CurrentTool as Slingshot).aimPos;
            aimPos.Value = Reflect(Utils.GetCorrectFarmerPosition(who), aimPos.Value);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static void Postfix(Farmer who)
        {
            if (!who.usingSlingshot || !who.IsLocalPlayer)
            {
                return;
            }

            NetPoint aimPos = (who.CurrentTool as Slingshot).aimPos;
            aimPos.Value = Reflect(Utils.GetCorrectFarmerPosition(who), aimPos.Value);
        }

        private static Point Reflect(Vector2 center, Point position)
        {
            Vector2 direction = new Vector2(position.X, position.Y) - center;

            return new Point((int)(center.X - direction.X), (int)(center.Y - direction.Y));
        }
    }
}
