using SObject = StardewValley.Object;

namespace StackEverything.ObjectCopiers
{
    /// <summary>Can make copies of objects.</summary>
    internal interface IObjectCopier
    {
        bool CanCopy(SObject obj);

        SObject Copy(SObject obj);
    }
}