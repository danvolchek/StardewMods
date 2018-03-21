using System;

namespace CustomizableDeathPenalty
{
    class ModConfig
    {
        public Boolean KeepItems { get; set; } = true;
        public Boolean KeepMoney { get; set; } = false;
        public Boolean RememberMineLevels { get; set; } = false;
        public Boolean RestoreStamina { get; set; } = false;
        public Boolean RestoreHealth { get; set; } = false;
    }
}
