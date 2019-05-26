namespace WinterGrass.LegacySaving
{
    /// <summary>A slimmed down version of SDV's Grass class with the relevant information needed to re-create it</summary>
    internal class LegacyGrassSave
    {
        public readonly int GrassType;
        public readonly int NumWeeds;

        public LegacyGrassSave(int w, int n)
        {
            this.GrassType = w;
            this.NumWeeds = n;
        }
    }
}