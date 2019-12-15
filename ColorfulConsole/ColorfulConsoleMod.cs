using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorfulConsole
{
    /// <summary>
    /// Represents the geode info menu mod.
    /// </summary>
    public class ColorfulConsole : Mod
    {
        /***
         * Private Fields
         ***/

        
        /// <summary>
        /// Entry method. Sets up config and event listeners.
        /// </summary>
        /// <param name="helper">Mod helper to read config and load sprites.</param>
        public override void Entry(IModHelper helper)
        {
           Console.SetOut(new ColorfulTextWriter(Console.Out));
        }

        
    }
}
