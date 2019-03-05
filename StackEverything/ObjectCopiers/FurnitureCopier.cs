using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace StackEverything.ObjectCopiers
{
    /// <summary>Copies furniture, orienting it in the right direction.</summary>
    internal class FurnitureCopier : IObjectCopier
    {
        public bool CanCopy(SObject obj)
        {
            return obj is Furniture;
        }

        public SObject Copy(SObject obj)
        {
            Furniture toCopy = obj as Furniture;
            Furniture furniture = toCopy.getOne() as Furniture;

            // Attempting to copy the rotation of the placed object is awful.
            // Try to match up the bounding boxes, giving up after 8 failed attempts.
            int attempts = 0;
            while (!furniture.boundingBox.Value.Equals(toCopy.boundingBox.Value) && attempts < 8)
            {
                furniture.rotate();
                attempts++;
            }

            return furniture;
        }
    }
}