namespace BetterDoors.Framework.ContentPacks
{
    /// <summary>
    /// An entry in a content pack providing a door sprite.
    /// </summary>
    internal class ContentPackDoorEntry
    {
        public string ImageFilePath { get; set; }
        public int TopLeftTileIndex { get; set; }
        public string Name { get; set; }

        public ContentPackDoorEntry(string imageFilePath, int topLeftTileIndex, string name)
        {
            this.ImageFilePath = imageFilePath;
            this.TopLeftTileIndex = topLeftTileIndex;
            this.Name = name;
        }
    }
}