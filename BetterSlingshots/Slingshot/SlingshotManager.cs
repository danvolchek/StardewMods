using Netcode;
using StardewModdingAPI;
using StardewValley;

namespace BetterSlingshots.Slingshot
{
    internal class SlingshotManager : IActionButtonAware
    {
        private readonly BetterSlingshotsConfig config;
        private bool isActionButtonDown;
        private StardewValley.Tools.Slingshot preparedSlingshot;

        private readonly IReflectionHelper reflection;
        private BetterSlingshot slingshotBeingUsed;

        public SlingshotManager(BetterSlingshotsConfig config, IReflectionHelper reflection)
        {
            this.reflection = reflection;
            this.config = config;
        }

        public void SetActionButtonDownState(bool which)
        {
            this.isActionButtonDown = which;
            this.slingshotBeingUsed?.SetActionButtonDownState(which);
        }

        /// <summary>Replaces the active slingshot (regular) with a better slingshot.</summary>
        public void PrepareForFiring()
        {
            this.preparedSlingshot = Game1.player.CurrentTool as StardewValley.Tools.Slingshot;

            this.slingshotBeingUsed = new BetterSlingshot(this.reflection, this.config,
                this.preparedSlingshot.attachments[0], this.isActionButtonDown,
                this.preparedSlingshot.InitialParentTileIndex);

            this.slingshotBeingUsed.PrepareForFiring();
            this.slingshotBeingUsed.beginUsing(Game1.currentLocation, Game1.getMouseX(), Game1.getMouseY(),
                Game1.player);


            Game1.player.CurrentTool = this.slingshotBeingUsed;
        }

        /// <summary>Replaces the active slingshot (better) with a regular slingshot.</summary>
        public void FiringOver()
        {
            this.preparedSlingshot.attachments[0] = this.slingshotBeingUsed.attachments[0];
            Game1.player.CurrentTool = this.preparedSlingshot;
            this.reflection.GetField<NetEvent0>(this.slingshotBeingUsed, "finishEvent").GetValue().Fire();
            this.reflection.GetField<NetEvent0>(this.preparedSlingshot, "finishEvent").GetValue().Fire();

        }

        /// <summary> Converts SDV's static int types to <see cref="SlingshotType" />.</summary>
        public static SlingshotType GetTypeFromIndex(int index)
        {
            switch (index)
            {
                default:
                case StardewValley.Tools.Slingshot.basicSlingshot:
                    return SlingshotType.Basic;

                case StardewValley.Tools.Slingshot.masterSlingshot:
                    return SlingshotType.Master;

                case StardewValley.Tools.Slingshot.galaxySlingshot:
                    return SlingshotType.Galaxy;
            }
        }
    }
}