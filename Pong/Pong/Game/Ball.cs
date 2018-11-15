using Pong.Framework.Enums;
using Pong.Framework.Game;
using Pong.Framework.Game.States;
using StardewValley;

namespace Pong.Game
{
    internal sealed class Ball : Collider, IReactiveDrawableCollideable
    {
        private readonly VelocityState velocityState;

        public Ball(PositionState positionState, VelocityState velocityState) : base(positionState, false)
        {
            this.velocityState = velocityState;
            this.Width = this.Height = Game1.tileSize;
        }

        public void CollideWith(INonReactiveDrawableCollideable other)
        {
            CollideInfo info = other.GetCollideInfo(this);

            if (info.Orientation == Orientation.Horizontal)
            {
                this.velocityState.YVelocity *= -1;

                if (info.CollidePercentage >= 0) this.velocityState.XVelocity = (int)(30 * info.CollidePercentage - 15);
            }
            else
            {
                this.velocityState.XVelocity *= -1;
            }
        }

        public void Reset()
        {
            this.velocityState.Reset();
            this.PositionState.Reset();
        }

        public override void Update()
        {
            this.XPos += this.velocityState.XVelocity;
            this.YPos += this.velocityState.YVelocity;
        }
    }
}