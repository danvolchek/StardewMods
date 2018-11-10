namespace Pong.Framework.Interfaces
{
    internal interface IReactiveDrawableCollideable : IDrawableCollideable
    {
        void CollideWith(INonReactiveDrawableCollideable other);
    }
}
