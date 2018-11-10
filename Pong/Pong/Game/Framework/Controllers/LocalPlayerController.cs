using StardewModdingAPI.Events;

namespace Pong.Game.Framework.Controllers
{
    internal class LocalPlayerController : PaddleController
    {
        private int lastXMousePos;

        public LocalPlayerController()
        {
            ControlEvents.MouseChanged += this.MouseChanged;
        }

        public override void Update()
        {
            this.IntendedPosition = this.lastXMousePos;
        }

        private void MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            this.lastXMousePos = e.NewPosition.X;
        }
    }
}