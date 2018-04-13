using Microsoft.Xna.Framework;
using SafeLightning.LightningProtection;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    /// Prints the times lightning will strike today.
    /// </summary>
    internal class GetLightningCommand : BaseCommand
    {
        public GetLightningCommand() : base("get_lightning", "gl", "Prints when lightning will harm terrain features today.")
        {
        }

        public override string Parse(string[] args)
        {
            string result = "Okay, lightning strikes today will be at:";
            for (int i = 600; i < 2400; i = SafeLightningMod.GetNextTime(i))
            {
                if (SDVLightningMimic.GetSDVLightningStrikePositionAt(new LightningStrikeRNGInfo(i), out KeyValuePair<Vector2, TerrainFeature>? feature))
                    result += $"\nLightning strike: {i} at {feature.Value.Key} on {feature.Value.Value.GetType().Name}.";
            }
            return result;
        }
    }
}