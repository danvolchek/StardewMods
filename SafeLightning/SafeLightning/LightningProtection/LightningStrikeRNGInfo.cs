using StardewValley;
using System;

namespace SafeLightning.LightningProtection
{
    /// <summary>
    /// Holds all the values used to create the <see cref="Random"/> used when determining what is hit by lightning, plus other fields used
    /// by the game when determing lightning strikes.
    /// </summary>
    internal class LightningStrikeRNGInfo
    {
        public uint daysPlayed;
        public double dailyLuck;
        public int luckLevel;
        public int time;
        public bool isLightning;

        /// <summary>
        /// Gets the current lightning RNG conditions.
        /// </summary>
        public LightningStrikeRNGInfo()
        {
            this.daysPlayed = Game1.stats.DaysPlayed;
            this.dailyLuck = Game1.dailyLuck;
            this.luckLevel = Game1.player.luckLevel;
            this.time = Game1.timeOfDay;
            this.isLightning = Game1.isLightning;
        }

        /// <summary>
        /// Gets the current lightning RNG conditions, but in the next 10 minutes.
        /// </summary>
        /// <param name="next">Increment the current time by 10 minutes</param>
        public LightningStrikeRNGInfo(bool next) : this()
        {
            if (next)
                time = SafeLightningMod.GetNextTime(this.time);
        }

        /// <summary>
        /// Gets the current lightning RNG conditions, but at a certain time.
        /// </summary>
        /// <param name="when">The time to use</param>
        public LightningStrikeRNGInfo(int when) : this()
        {
            time = when;
        }

        /// <summary>
        /// Gets the Random used to choose lightning position for this info.
        /// </summary>
        /// <returns>A Random</returns>
        public Random GetRandom()
        {
            return new Random((int)Game1.uniqueIDForThisGame + (int)this.daysPlayed + this.time);
        }
    }
}