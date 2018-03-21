namespace Pong.Game
{
    interface INonReactiveCollideable : ICollideable
    {
        CollideInfo GetCollideInfo(IReactiveCollideable other);

        void Resize();
    }
}
