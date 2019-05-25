using System;
using StardewValley.Tools;

namespace BetterSlingshots.Framework.Patching
{
    internal class SlingshotFiringEventArgs : EventArgs
    {
        public Slingshot Slingshot { get; }

        public SlingshotFiringEventArgs(Slingshot slingshot)
        {
            this.Slingshot = slingshot;
        }
    }
}
