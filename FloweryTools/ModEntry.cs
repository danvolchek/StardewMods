using FloweryTools.Framework.Creators;
using FloweryTools.Framework.Flowerers;
using FloweryTools.ParticleCreator;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Reflection;

namespace FloweryTools
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private int lastToolPower;
        private bool lastUsingSlingshot;
        private bool lastcastedButBobberStillInAir;

        private IList<IToolFlowerer> flowerers;

        private IParticleCreator explosionCreator;
        private IParticleCreator slingshotCreator;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModConfig config = this.Helper.ReadConfig<ModConfig>();

            Multiplayer multiplayer = typeof(Game1).GetField("multiplayer", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Multiplayer;

            FlowerHelper flowerHelper = new FlowerHelper(multiplayer, config.LocalOnly);

            flowerers = new List<IToolFlowerer> {
                new Swipe(flowerHelper),
                new Swing(flowerHelper),
                new Stab(flowerHelper),
                new Defense(flowerHelper),
                new Watering(flowerHelper),
                new TimingCast(flowerHelper)
            };

            explosionCreator = new Explosion(flowerHelper);
            slingshotCreator = new Slingshot(flowerHelper);

            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        /*********
        ** Private methods
        *********/
        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // Handle all normal animation based tools
            foreach (IToolFlowerer flowerer in this.flowerers) {
                if (flowerer.Matches(Game1.player.FarmerSprite.timer, Game1.player.FarmerSprite.CurrentAnimation))
                {
                    flowerer.CreateParticles(Game1.player.currentLocation, Game1.player.FarmerSprite.currentAnimationIndex);
                }
            }

            // I like having flowers burst out when charge level changes, but this doesn't happen through animations like above.
            if (lastToolPower != Game1.player.toolPower && !(lastToolPower == 0 && Game1.player.toolPower == 3))
            {
                this.explosionCreator.CreateParticles(Game1.player.currentLocation, 0);
            }
            this.lastToolPower = Game1.player.toolPower;

            // Same for slingshots.
            if (!Game1.player.usingSlingshot && lastUsingSlingshot)
            {
                this.slingshotCreator.CreateParticles(Game1.player.currentLocation, 0);
            }
            this.lastUsingSlingshot = Game1.player.usingSlingshot;

            // Same for the bobber landing.
            if (Game1.player.CurrentTool is StardewValley.Tools.FishingRod rod)
            {
                if (!rod.castedButBobberStillInAir && lastcastedButBobberStillInAir)
                    this.explosionCreator.CreateParticles(Game1.player.currentLocation, 0);

                this.lastcastedButBobberStillInAir = rod.castedButBobberStillInAir;
            } else
            {
                this.lastcastedButBobberStillInAir = false;
            }
        }
    }
}