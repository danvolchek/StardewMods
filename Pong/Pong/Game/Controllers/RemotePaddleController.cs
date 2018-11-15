using Pong.Framework.Game.States;

namespace Pong.Game.Controllers
{
    internal class RemotePaddleController : IntentionalPaddleController
    {
        private readonly PositionState intendedState;

        public RemotePaddleController(PositionState intendedState)
        {
            this.intendedState = intendedState;
        }

        public override void Update()
        {
            this.IntendedPosition = this.intendedState.XPosition;
        }
    }
}
