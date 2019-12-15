using Microsoft.Xna.Framework;

namespace Pong.Game.Controllers
{
    internal class ComputerController : IntentionalPaddleController
    {
        private readonly Ball ball;

        public ComputerController(Ball ball)
        {
            this.ball = ball;
        }

        public override void Update()
        {
            Rectangle boundingBox = this.ball.Bounds;
            this.IntendedPosition = boundingBox.X + boundingBox.Width / 2;
        }
    }
}
