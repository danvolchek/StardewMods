namespace WinterGrass.LegacySaving
{
    /// <summary>A slimmed down version of SDV's Grass class with the relevant information needed to re-create it.</summary>
    internal class LegacyGrassSave
    {
        /*********
        ** Fields
        *********/

        /// <summary>The type of the grass.</summary>
        public readonly int GrassType;

        /// <summary>The number of weeds the grass has.</summary>
        public readonly int NumWeeds;

        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="grassType">The type of the grass.</param>
        /// <param name="numWeeds">The number of weeds the grass has.</param>
        public LegacyGrassSave(int grassType, int numWeeds)
        {
            this.GrassType = grassType;
            this.NumWeeds = numWeeds;
        }
    }
}
