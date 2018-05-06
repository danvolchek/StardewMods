using SObject = StardewValley.Object;

namespace StackEverything.ObjectCopiers
{
    /// <summary>Copies a tapper, making sure to keep its <see cref="SObject.heldObject"/> and <see cref="SObject.minutesUntilReady"/>.</summary>
    internal class TapperCopier : IObjectCopier
    {
        public bool CanCopy(SObject obj)
        {
            return obj.Name == "Tapper";
        }

        public SObject Copy(SObject obj)
        {
            SObject copy = (SObject)obj.getOne();
            copy.heldObject.Value = obj.heldObject.Value;
            copy.minutesUntilReady.Value = obj.minutesUntilReady.Value;
            copy.quality.Value = obj.quality.Value;
            return copy;
        }
    }
}