using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SafeLightning.API;
using SafeLightning.CommandParsing;
using SafeLightning.LightningProtection;
using SafeLightning.LightningProtection.ResultDetectors;
using SafeLightning.LightningProtection.ResultResolvers;
using SafeLightning.LightningProtection.ResultResolvers.SavedFeatureData;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SafeLightning
{
    /// <summary>
    ///     A mod which prevents negative affects from lightning hitting anything but lightning rods.
    /// </summary>
    /// <remarks>
    ///     How this mod actually works is by detecting when something is hit by lightning,
    ///     restoring it to its original state, and removing any side effects of the restoration.
    /// </remarks>
    public class SafeLightningMod : Mod
    {
        /// <summary>
        ///     Delegate for the method the API will call when it is notified about a new strike.
        /// </summary>
        /// <param name="position">The position to be hit</param>
        /// <param name="effects">Whether to display visual/sound effects or not</param>
        public delegate void StrikeLightningDelegate(Vector2 position, bool effects);

        /// <summary>
        ///     <see cref="TerrainFeature" />s, or copies of them if needed, befor they were hit by lightning.
        /// </summary>
        private readonly IDictionary<Vector2, BaseFeatureSaveData> featuresBeforeTheyWereHit =
            new Dictionary<Vector2, BaseFeatureSaveData>();

        /// <summary>
        ///     Margin used to decide when to start saving <see cref="TerrainFeature" />s.
        /// </summary>
        /// <remarks>See <see cref="SafeLightningMod.UpdateTick(object, EventArgs)" /> for why this is needed.</remarks>
        private readonly int margin = 100;

        /// <summary>
        ///     Is listening to events.
        /// </summary>
        private bool isSubscribed;

        /// <summary>
        ///     Detectors that detect negative lightning strike results.
        /// </summary>
        private List<IResultDetector> lightningDetectors;

        /// <summary>
        ///     Resolvers that fix negative lightning strike results.
        /// </summary>
        private IDictionary<LightningStrikeResult, IResultResolver> lightningResolvers;

        /// <summary>
        ///     Whether a mod caused a lightning strike that we need to recover from instantly.
        /// </summary>
        private bool modForcedRestore;

        /// <summary>
        ///     Used to simulate overnight strikes correctly.
        /// </summary>
        private LightningStrikeRNGInfo previousDayRNGInfo;

        /// <summary>
        ///     Last <see cref="Game1.timeOfDay" /> <see cref="TerrainFeature" />s were saved at.
        /// </summary>
        private int savedFeaturesAt = -1;

        /// <summary>
        ///     Safe Lightning initialization. Sets up console commands, lightning detectors and resolvers, and subscribes to
        ///     events.
        /// </summary>
        /// <param name="helper">The mod helper</param>
        public override void Entry(IModHelper helper)
        {
            new CommandParser(this);

            //Add detectors
            this.lightningDetectors = new List<IResultDetector>
            {
                new CropKilledDetector(),
                new FruitTreeCoalDetector(),
                new RemovedFeatureDetector(),
                new TreeFallingDetector(helper.Reflection)
            };

            this.Monitor.Log($"Loaded {this.lightningDetectors.Count} detectors.", LogLevel.Trace);

            //Add resolvers
            IResultResolver crop = new CropKilledResolver(this.Monitor);
            IResultResolver fruit = new FruitTreeCoalResolver(this.Monitor);
            IResultResolver removed = new RemovedFeatureResolver(this.Monitor);
            IResultResolver falling = new TreeFallingResolver(helper.Reflection, this.Monitor);
            this.lightningResolvers = new Dictionary<LightningStrikeResult, IResultResolver>
            {
                {crop.Result, crop},
                {fruit.Result, fruit},
                {removed.Result, removed},
                {falling.Result, falling}
            };

            this.Monitor.Log($"Loaded {this.lightningDetectors.Count} resolvers.", LogLevel.Trace);

            SaveEvents.AfterLoad += this.AfterLoad;
            SaveEvents.AfterReturnToTitle += (sender, args) => this.Unsubscribe();
        }

        private void AfterLoad(object sender, EventArgs e)
        {
            if (!Context.IsMainPlayer)
                this.Unsubscribe();
            else
                this.Subscribe();
        }

        private void Subscribe()
        {
            if (this.isSubscribed)
                return;

            GameEvents.UpdateTick += this.UpdateTick;
            SaveEvents.BeforeSave += this.BeforeSave;

            this.Monitor.Log("Subscribed to events.", LogLevel.Trace);
            this.isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!this.isSubscribed)
                return;

            GameEvents.UpdateTick -= this.UpdateTick;
            SaveEvents.BeforeSave -= this.BeforeSave;

            this.savedFeaturesAt = -1;

            this.Monitor.Log("Unsubscribed from events.", LogLevel.Trace);
            this.isSubscribed = false;
        }

        /// <summary>
        ///     Restores a location to what it was before a lightning strike given which features were affected.
        /// </summary>
        /// <param name="location">The <see cref="GameLocation" /> to restore</param>
        /// <param name="affectedFeatures">Features affected by lightning strikes last tick</param>
        /// <returns>Whether all affected features were restored</returns>
        private bool RestoreLocation(GameLocation location, IDictionary<Vector2, BaseFeatureSaveData> affectedFeatures)
        {
            if (affectedFeatures.Count == 0)
                return true;

            this.Monitor.Log($"Restore - {affectedFeatures.Count} features to resolve.", LogLevel.Trace);

            IList<Vector2> toRemove = new List<Vector2>();

            //If a detector detects an affected feature, send the feature to the corresponding resolver to fix it.
            foreach (IResultDetector detector in this.lightningDetectors)
            {
                foreach (Vector2 featurePosition in detector.Detect(location, affectedFeatures.Keys))
                {
                    this.lightningResolvers[detector.Result].Resolve(location, affectedFeatures[featurePosition]);
                    toRemove.Add(featurePosition);
                }

                foreach (Vector2 item in toRemove)
                    affectedFeatures.Remove(item);
                toRemove.Clear();
            }

            return affectedFeatures.Count == 0;
        }

        /// <summary>
        ///     Calls <see cref="SafeLightningMod.RestoreLocation(GameLocation, IDictionary{Vector2, BaseFeatureSaveData})" /> but
        ///     ignores unresolved <see cref="TerrainFeature" />s.
        ///     This shouldn't happen, but we account for it anyway.
        /// </summary>
        /// <param name="location">The <see cref="GameLocation" /> to restore</param>
        /// <param name="affectedFeatures">The affected <see cref="TerrainFeature" />s</param>
        private void RestoreLocationAndIgnoreErrors(GameLocation location,
            IDictionary<Vector2, BaseFeatureSaveData> affectedFeatures)
        {
            if (!this.RestoreLocation(Game1.getFarm(), affectedFeatures))
            {
                this.Monitor.Log($"Failed to restore some features. They're being ignored.", LogLevel.Trace);
                this.featuresBeforeTheyWereHit.Clear();
            }
        }

        /// <summary>
        ///     Handles the next update tick. Restores any affected <see cref="TerrainFeature" />s and saves
        ///     <see cref="TerrainFeature" />s to be hit in the next ten minutes.
        /// </summary>
        private void UpdateTick(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (this.savedFeaturesAt != -1 && this.savedFeaturesAt != Game1.timeOfDay || this.modForcedRestore)
            {
                this.RestoreLocationAndIgnoreErrors(Game1.getFarm(), this.featuresBeforeTheyWereHit);

                if (this.modForcedRestore) this.modForcedRestore = false;
                this.savedFeaturesAt = -1;
            }

            //Lightning protection works by saving features right before lightning hits them, i.e. right before the game's ten minute update.
            //Unfortunately, there is no reliable way to determine when the current tick is the last tick before the ten minute update.
            //We use a margin based on the game's original code to save features. The margin is large enough to be compatible with mods that speed up
            //time up to ten in game minutes == 1 real life second, but save more than necessary for the default ten minute length.
            if (Game1.gameTimeInterval >=
                7000 + Game1.currentLocation.getExtraMillisecondsPerInGameMinuteForThisLocation() - this.margin)
            {
                if (SDVLightningMimic.GetSDVLightningStrikePositionAt(new LightningStrikeRNGInfo(true),
                    out KeyValuePair<Vector2, TerrainFeature> item))
                    this.SaveFeature(item.Key, item.Value);

                this.savedFeaturesAt = Game1.timeOfDay;
            }

            if (Game1.newDay) this.PrepareForOvernightLightning();
        }

        /// <summary>
        ///     Save the given <see cref="TerrainFeature" /> state so it can be restored later.
        /// </summary>
        /// <remarks>
        ///     <see cref="IResultResolver" />s may require information about the <see cref="TerrainFeature" /> before it was
        ///     modified, so save that if necessary.
        /// </remarks>
        /// <param name="pos">Where the <see cref="TerrainFeature" /> is</param>
        /// <param name="value">The <see cref="TerrainFeature" /> itself</param>
        private void SaveFeature(Vector2 pos, TerrainFeature value)
        {
            this.featuresBeforeTheyWereHit[pos] =
                FeatureSaveDataFactory.CreateFeatureSaveData(pos, value, this.Helper.Reflection);
        }

        /// <summary>
        ///     Gets the in game time 10 minutes after when.
        /// </summary>
        /// <param name="when">The time</param>
        /// <returns>10 minutes after the given time</returns>
        internal static int GetNextTime(int when)
        {
            when += 10;
            if (when % 100 >= 60)
                when = when - when % 100 + 100;
            return when;
        }

        /*********
         * Overnight Handling
         *********/

        /// <summary>
        ///     If there will be lightning tonight, prevent it and save the conditions that decide which
        ///     <see cref="TerrainFeature" />s get hit.
        /// </summary>
        private void PrepareForOvernightLightning()
        {
            if (Game1.isLightning)
            {
                this.previousDayRNGInfo = new LightningStrikeRNGInfo();
                Game1.isLightning = false;
                this.Monitor.Log($"Turned off lightning. Player went to bed at {Game1.timeOfDay}.", LogLevel.Trace);
            }
        }

        /// <summary>
        ///     Before a save, simulate any lightning the previous night should have had safely.
        /// </summary>
        private void BeforeSave(object sender, EventArgs e)
        {
            if (this.previousDayRNGInfo == null)
                return;

            //Fix Game1.wasRainingYesterday
            Game1.wasRainingYesterday = Game1.wasRainingYesterday || this.previousDayRNGInfo.isLightning;

            int num = (2400 - this.previousDayRNGInfo.time) / 100;

            this.Monitor.Log($"Running overnight lightning {num} times.", LogLevel.Trace);

            for (int i = 0; i < num; i++)
            {
                if (SDVLightningMimic.GetSDVLightningStrikePositionAt(this.previousDayRNGInfo,
                    out KeyValuePair<Vector2, TerrainFeature> item))
                {
                    this.SaveFeature(item.Key, item.Value);
                    this.Monitor.Log($"{item.Value.GetType().Name} at {item.Key} will be hit next.", LogLevel.Trace);
                }

                SDVLightningMimic.CauseVanillaStrike(this.previousDayRNGInfo);
                this.RestoreLocationAndIgnoreErrors(Game1.getFarm(), this.featuresBeforeTheyWereHit);
            }

            this.previousDayRNGInfo = null;
        }

        /*********
         * Mod API
         *********/

        /// <summary>
        ///     Returns a new instance of the API.
        /// </summary>
        /// <returns>This mod's api</returns>
        public override object GetApi()
        {
            return new SafeLightningApi(this.ModWantsToStrikeLightningAt);
        }

        /// <summary>
        ///     When told that another mod will cause a lightning strike, save the state of that <see cref="TerrainFeature" /> so
        ///     it can be restored.
        /// </summary>
        /// <param name="position">The position that will be hit</param>
        /// <param name="effects">Whether visual/sound effects should be created</param>
        private void ModWantsToStrikeLightningAt(Vector2 position, bool effects)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.getFarm().terrainFeatures.TryGetValue(position, out TerrainFeature feature))
            {
                this.SaveFeature(position, feature);
                this.modForcedRestore = true;
            }

            SDVLightningMimic.StrikeLightningSafelyAt(this.Monitor, position, effects);
        }
    }
}