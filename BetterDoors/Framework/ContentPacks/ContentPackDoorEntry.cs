namespace BetterDoors.Framework.ContentPacks
{
    /// <summary>An entry in a content pack providing a door animation.</summary>
    internal class ContentPackDoorEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The relative file path to the image.</summary>
        public string ImageFilePath { get; set; }

        /// <summary>The top left tile index of the animation.</summary>
        public int TopLeftTileIndex { get; set; }

        /// <summary>The name of the door animation.</summary>
        public string Name { get; set; }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="imageFilePath">The relative file path to the image.</param>
        /// <param name="topLeftTileIndex">The top left tile index of the animation.</param>
        /// <param name="name">The name of the door animation.</param>
        public ContentPackDoorEntry(string imageFilePath, int topLeftTileIndex, string name)
        {
            this.ImageFilePath = imageFilePath;
            this.TopLeftTileIndex = topLeftTileIndex;
            this.Name = name;
        }
    }
}