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

            Furniture furniture = new Furniture(toCopy.parentSheetIndex, toCopy.tileLocation)
            {
                defaultBoundingBox = toCopy.defaultBoundingBox,
                boundingBox = toCopy.boundingBox,
                rotations = toCopy.rotations,
                currentRotation = 0,
                quality = toCopy.quality
            };

            furniture.updateDrawPosition();

            if (toCopy.rotations == 2)
                furniture.currentRotation = toCopy.currentRotation;

            for (int i = 0; i < toCopy.currentRotation; i++)
                furniture.rotate();

            furniture.initializeLightSource(toCopy.tileLocation);
            return furniture;
        }
    }
}