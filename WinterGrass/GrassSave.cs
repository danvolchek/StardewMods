namespace WinterGrass
{
    //A slimmed down version of SDV's Grass class with the relevant information needed to re-create it
    internal class GrassSave
    {
        public readonly int GrassType;
        public readonly int NumWeeds;

        public GrassSave(int w, int n)
        {
            this.GrassType = w;
            this.NumWeeds = n;
        }
    }
}