namespace Pong.Framework.Game
{
    internal interface IState<T>: IResetable
    {
        void SetState(T state);
    }
}
