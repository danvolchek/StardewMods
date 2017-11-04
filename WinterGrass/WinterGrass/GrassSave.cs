using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterGrass
{
    //A slimmed down version of SDV's Grass class with the relevant information needed to re-create it
    class GrassSave
    {
        public int grassType;
        public int numWeeds;


        public GrassSave(int w, int n)
        {
            grassType = w;
            numWeeds = n;
        }
    }
}
