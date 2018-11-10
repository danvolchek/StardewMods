namespace Pong.Game.Interfaces
{
    interface IReactiveCollideable : ICollideable
    {
        void CollideWith(INonReactiveCollideable other);
    }
}
