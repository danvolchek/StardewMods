using StardewValley.Objects;

namespace StackEverything.ObjectCopiers
{
    /// <summary>Copies furniture, orienting it in the right direction.</summary>
    internal class FurnitureCopier : ICopier<Furniture>
    {
        /*********
        ** Public methods
        *********/

        /// <summary>Copy the given object.</summary>
        /// <param name="obj">The object to copy</param>
        /// <returns>A copy of the object.</returns>
        public Furniture Copy(Furniture obj)
        {
            Furniture furniture = obj.getOne() as Furniture;

            // Attempting to copy the rotation of the placed object is awful.
            // Try to match up the bounding boxes, giving up after 8 failed attempts.
            int attempts = 0;
            while (!furniture.boundingBox.Value.Equals(obj.boundingBox.Value) && attempts < 8)
            {
                furniture.rotate();
                attempts++;
            }

            return furniture;
        }
    }
}
