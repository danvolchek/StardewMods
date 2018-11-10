namespace Pong.Game.Interfaces
{
    interface INonReactiveCollideable : ICollideable
    {
        CollideInfo GetCollideInfo(IReactiveCollideable other);

        void Resize();
    }
}
