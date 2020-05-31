using FloweryTools.Framework.Flowerers;
using FloweryTools.ParticleCreator;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FloweryTools.Framework.Creators
{
    // Used when slingshots fire
    class Slingshot : BaseCreator, IParticleCreator
    {
        public Slingshot(FlowerHelper helper) : base(helper)
        {

        }

        public void CreateParticles(GameLocation location, int frameIndex)
        {
            if (!(Game1.player.CurrentTool is StardewValley.Tools.Slingshot slingshot) || slingshot.attachments.Length == 0 || slingshot.attachments[0] == null || slingshot.attachments[0].Stack <= 0 || slingshot.mouseDragAmount <= 4)
                return;

            int times = this.helper.rand.Next(1, 3);
            for (int i = 0; i < times; i++)
            {
                this.helper.AddFlower(this.helper.ApplyJitter(this.helper.GetOffset(1), 10), this.helper.ApplyJitter(this.GetMotion(Game1.player, slingshot), 0.1f), location);
            }
        }

        // Loosly taken from Slingshot. It's not perfect but it's close enough, and simple to read.
        private Vector2 GetMotion(Farmer farmer, StardewValley.Tools.Slingshot slingshot)
        {
            Vector2 motion = Utility.getVelocityTowardPoint(new Vector2((float)slingshot.aimPos.X, (float)slingshot.aimPos.Y), new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY() + 32), 256f);
            motion.Normalize();
            motion *= 0.3f;

            return motion;
        }
    }
}
