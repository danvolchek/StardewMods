using Pong.Framework.Game.States;

namespace Pong.Game.Controllers
{
    internal class StatePaddleController : IntentionalPaddleController
    {
        protected readonly PositionState intendedState;

        public StatePaddleController(PositionState intendedState)
        {
            this.intendedState = intendedState;
        }

        public override void Update()
        {
            this.IntendedPosition = this.intendedState.XPosition;
        }
    }
}
