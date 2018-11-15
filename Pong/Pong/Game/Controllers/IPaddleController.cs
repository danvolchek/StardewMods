using Pong.Framework.Common;

namespace Pong.Game.Controllers
{
    internal interface IPaddleController : IUpdateable
    {
        int GetMovement(int xPos, int width);
    }
}
