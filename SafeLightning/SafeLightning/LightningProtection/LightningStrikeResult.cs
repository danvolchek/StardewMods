namespace SafeLightning.LightningProtection
{
    /// <summary>
    /// Represents the results that can occur when a <see cref="StardewValley.TerrainFeatures.TerrainFeature"/> is hit by a lightning strike.
    /// </summary>
    internal enum LightningStrikeResult
    {
        Removed,
        CropKilled,
        FruitTreeTurnedToCoal,
        TreeFalling
    }
}