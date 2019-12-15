using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BetterSlingshots.Framework.Patches
{
    /// <summary>Draws the aiming reticules in the right spots.</summary>
    [HarmonyPatch]
    internal class SlingshotDrawPatch
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static MethodBase TargetMethod()
        {
            return typeof(Slingshot).GetMethod(nameof(Slingshot.draw));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Argument names are defined by Harmony.")]
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Method names are defined by Harmony.")]
        private static bool Prefix(Slingshot __instance, SpriteBatch b)
        {
            SlingshotDrawPatch.DrawReticules(__instance, b);

            return false;
        }

        private static void DrawReticules(Slingshot slingshot, SpriteBatch spriteBatch)
        {
            Farmer lastFarmer = slingshot.getLastFarmerToUse();
            if (lastFarmer == null || !lastFarmer.usingSlingshot || !lastFarmer.IsLocalPlayer)
                return;

            if (BetterSlingshotsMod.Instance.Config.ShowActualMousePositionWhenAiming)
            {
                SlingshotDrawPatch.DrawTargetingReticule(spriteBatch, new Vector2(Game1.getMouseX(), Game1.getMouseY()));
            }

            Vector2 projectileVelocity = Utils.GetCorrectVelocity(new Vector2(slingshot.aimPos.X, slingshot.aimPos.Y), lastFarmer, !BetterSlingshotsMod.Instance.Config.DisableReverseAiming);
            projectileVelocity.Normalize();

            Vector2 farmerPosition = Utils.GetCorrectFarmerPosition(lastFarmer);

            SlingshotDrawPatch.DrawTargetingReticule(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(farmerPosition.X + projectileVelocity.X * 250, farmerPosition.Y + projectileVelocity.Y * 250)));
        }

        private static void DrawTargetingReticule(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(Game1.mouseCursors, position, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43, -1, -1)), Color.White, 0.0f, new Vector2(32f, 32f), 1f, SpriteEffects.None, 0.999999f);
        }
    }
}
