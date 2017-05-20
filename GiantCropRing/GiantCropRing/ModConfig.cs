using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiantCropRing
{
    class ModConfig
    {
        public double cropChancePercentWithRing { get; set; } = 0.05;
        public Boolean shouldWearingBothRingsDoublePercentage { get; set; } = true;

        public int cropRingPrice { get; set; } = 5000;
    }
}
