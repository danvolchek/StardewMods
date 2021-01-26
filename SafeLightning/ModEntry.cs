using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using SafeLightning.CommandParsing;
using StardewModdingAPI;
using StardewValley;
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
            var commandParser = new CommandParser(Monitor, Helper.ConsoleCommands);
            commandParser.RegisterCommands();
            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            //instance.PatchAll(Assembly.GetExecutingAssembly());
            harmony.Patch(AccessTools.Method(typeof(Utility), nameof(Utility.performLightningUpdate)),
                new HarmonyMethod(typeof(PerformLightningUpdatePatch), nameof(PerformLightningUpdatePatch.Prefix)));
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
            return new Api(ModWantsToStrikeLightningAt);
        }

        /// <summary>When told that another mod will cause a lightning strike, save the state of that <see cref="TerrainFeature" /> so it can be restored.</summary>
        /// <param name="position">The position that will be hit</param>
        /// <param name="effects">Whether visual/sound effects should be created</param>
        private static void ModWantsToStrikeLightningAt(Vector2 position, bool effects)
        {
            if (!Context.IsWorldReady) return;
            StrikeLightningSafelyAt(position, effects);
        }

        /// <summary>Safely strikes lightning.</summary>
        /// <param name="position">The position to strike lightning</param>
        /// <param name="effects">Whether visual/sound effects should be created</param>
        private static void StrikeLightningSafelyAt(Vector2 position, bool effects)
        {
            var farm = Game1.getFarm();
            var lightningEvent2 = new Farm.LightningStrikeEvent();
            if (farm.objects.TryGetValue(position, out var obj) && obj.bigCraftable.Value &&
                obj.ParentSheetIndex == 9 && obj.heldObject.Value == null)
            {
                obj.heldObject.Value = new Object(787, 1);
                obj.MinutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                obj.shakeTimer = 1000;
                lightningEvent2.bigFlash = true;
                lightningEvent2.createBolt = true;
                lightningEvent2.boltPosition = position * 64f + new Vector2(32f, 0.0f);
                return;
            }
            var lightningStrikeEvent = new Farm.LightningStrikeEvent {smallFlash = true};
            if (effects)
            {
                farm.lightningStrikeEvent.Fire(lightningStrikeEvent);
            }
        }
    }
}