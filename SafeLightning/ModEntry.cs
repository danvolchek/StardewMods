using System;
using HarmonyLib;
using SafeLightning.CommandParsing;
using StardewModdingAPI;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace SafeLightning
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            CommandParser commandParser = new CommandParser(this.Monitor, this.Helper.ConsoleCommands);
            commandParser.RegisterCommands();

            Harmony instance = new Harmony(this.Helper.ModRegistry.ModID);
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        /*********
        ** Mod API
        **********/
        /// <summary>Delegate for the method the API will call when it is notified about a new strike.</summary>
        /// <param name="position">The position to be hit</param>
        /// <param name="effects">Whether to display visual/sound effects or not</param>
        public delegate void StrikeLightningDelegate(Vector2 position, bool effects);

        /// <summary>Get an API that other mods can access. This is always called after <see cref="Entry" />.</summary>
        public override object GetApi()
        {
            return new API(this.ModWantsToStrikeLightningAt);
        }

        /// <summary>When told that another mod will cause a lightning strike, save the state of that <see cref="TerrainFeature" /> so it can be restored.</summary>
        /// <param name="position">The position that will be hit</param>
        /// <param name="effects">Whether visual/sound effects should be created</param>
        private void ModWantsToStrikeLightningAt(Vector2 position, bool effects)
        {
            if (!Context.IsWorldReady)
                return;

            StrikeLightningSafelyAt(position, effects);
        }

        /// <summary>Safely strikes lightning.</summary>
        /// <param name="position">The position to strike lightning</param>
        /// <param name="effects">Whether visual/sound effects should be created</param>
        internal static void StrikeLightningSafelyAt(Vector2 position, bool effects)
        {
            Farm farm = Game1.getFarm();

            Farm.LightningStrikeEvent lightningStrikeEvent;

            if (farm.objects.TryGetValue(position, out StardewValley.Object obj) && obj.bigCraftable.Value && obj.ParentSheetIndex == 9 && obj.heldObject.Value == null)
            {
                obj.heldObject.Value = new StardewValley.Object("787", 1, false, -1, 0);
                obj.MinutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                obj.shakeTimer = 1000;

                lightningStrikeEvent = new Farm.LightningStrikeEvent
                {
                    bigFlash = true,
                    createBolt = true,
                    boltPosition = position * 64f + new Vector2(32f, 0.0f)
                };
            }
            else 
            {
                lightningStrikeEvent = new Farm.LightningStrikeEvent()
                {
                    smallFlash = true
                };
            }

            if (effects)
            {
                farm.lightningStrikeEvent.Fire(lightningStrikeEvent);
            }
        }
    }
}
