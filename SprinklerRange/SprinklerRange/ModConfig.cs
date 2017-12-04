using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprinklerRange
{
    class ModConfig
    {
        public String SprinklerScarecrowActivationKey { get; set; } = "f2";
        public String BeeHouseActivationKey { get; set; } = "f3";
        public Boolean ShowRangeOfHeldSprinklerOrScarecrowOrBeehouse { get; set; } = true;
        public Boolean UseOldColorScheme { get; set; } = false;

    }
}
