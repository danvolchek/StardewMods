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

            Furniture furniture = new Furniture(toCopy.parentSheetIndex.Value, toCopy.tileLocation.Value);

            furniture.defaultBoundingBox.Value = toCopy.defaultBoundingBox;
            furniture.boundingBox.Value = toCopy.boundingBox;
            furniture.rotations.Value = toCopy.rotations;
            furniture.currentRotation.Value = 0;
            furniture.quality.Value = toCopy.quality;
            

            furniture.updateDrawPosition();

            if (toCopy.rotations == 2)
                furniture.currentRotation.Value = toCopy.currentRotation.Value;

            for (int i = 0; i < toCopy.currentRotation.Value; i++)
                furniture.rotate();

            furniture.initializeLightSource(toCopy.tileLocation.Value);
            return furniture;
        }
    }
}