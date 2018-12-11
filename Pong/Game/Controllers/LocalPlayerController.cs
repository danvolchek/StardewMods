using Pong.Framework.Game.States;
using StardewModdingAPI.Events;

namespace Pong.Game.Controllers
{
    internal class LocalPlayerController : StatePaddleController
    {
        public LocalPlayerController(PositionState intendedState) : base(intendedState)
        {
            ControlEvents.MouseChanged += this.MouseChanged;
        }

        private void MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            this.intendedState.XPosition = e.NewPosition.X;
        }
    }
}