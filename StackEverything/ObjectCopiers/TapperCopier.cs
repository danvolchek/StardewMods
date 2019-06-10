using SObject = StardewValley.Object;

namespace StackEverything.ObjectCopiers
{
    /// <summary>Copies a tapper, making sure to keep its <see cref="SObject.heldObject"/> and <see cref="SObject.minutesUntilReady"/>.</summary>
    internal class TapperCopier : ICopier<SObject>
    {
        public SObject Copy(SObject obj)
        {
            if (obj.Name != "Tapper")
            {
                return null;
            }

            SObject copy = (SObject)obj.getOne();
            copy.heldObject.Value = obj.heldObject.Value;
            copy.MinutesUntilReady = obj.MinutesUntilReady;
            copy.Quality = obj.Quality;
            return copy;
        }
    }
}