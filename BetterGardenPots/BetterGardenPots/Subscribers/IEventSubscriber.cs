namespace BetterGardenPots.Subscribers
{
    internal interface IEventSubscriber
    {
        void Subscribe();
        void Unsubscribe();
    }
}