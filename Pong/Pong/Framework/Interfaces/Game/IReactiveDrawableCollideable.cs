namespace Pong.Framework.Interfaces.Game
{
    internal interface IReactiveDrawableCollideable : IDrawableCollideable
    {
        void CollideWith(INonReactiveDrawableCollideable other);
    }
}
