using StardewModdingAPI;
using System;

namespace NoCrows
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            try { 
                Injector.Replace(typeof(StardewValley.Farm), "addCrows", typeof(ModEntry), "DoNothing");
            } catch(Exception e)
            {
                Monitor.Log("No Crows failed to initialize properly and won't work! Please add a bug report on the nexusmods page with the following info:", LogLevel.Error);
                Monitor.Log(e.Message, LogLevel.Error);
            }
        }

        public void DoNothing() {}
     
    }
}
