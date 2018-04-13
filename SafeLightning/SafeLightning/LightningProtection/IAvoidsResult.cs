namespace SafeLightning.LightningProtection
{
    /// <summary>
    /// Helps in avoiding lighting strike results.
    /// </summary>
    internal interface IAvoidsResult
    {
        LightningStrikeResult Result { get; }
    }
}