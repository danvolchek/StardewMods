using System;

namespace RangeDisplay
{
    public class RangeDisplayConfig
    {
        public String CycleActiveDisplayKey { get; set; } = "f2";
        public Boolean ShowRangeOfHeldItem { get; set; } = true;
        public Boolean ShowRangeOfHoveredOverItem { get; set; } = true;
        public String HoverModifierKey { get; set; } = "leftcontrol";
    }
}