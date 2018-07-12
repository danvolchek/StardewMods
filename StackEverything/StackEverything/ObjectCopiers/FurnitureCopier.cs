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

            Furniture furniture = new Furniture(toCopy.ParentSheetIndex, toCopy.TileLocation);

            furniture.defaultBoundingBox.Value = toCopy.defaultBoundingBox.Value;
            furniture.boundingBox.Value = toCopy.boundingBox.Value;
            furniture.rotations.Value = toCopy.rotations.Value;
            furniture.currentRotation.Value = 0;
            furniture.Quality = toCopy.Quality;
            

            furniture.updateDrawPosition();

            if (toCopy.rotations.Value == 2)
                furniture.currentRotation.Value = toCopy.currentRotation.Value;

            for (int i = 0; i < toCopy.currentRotation.Value; i++)
                furniture.rotate();

            furniture.initializeLightSource(toCopy.TileLocation);
            return furniture;
        }
    }
}