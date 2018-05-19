using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SafeLightning.LightningProtection;
using StardewValley.TerrainFeatures;

namespace SafeLightning.CommandParsing.Commands
{
    /// <summary>
    ///     Prints the times lightning will strike today.
    /// </summary>
    internal class GetLightningCommand : BaseCommand
    {
        public GetLightningCommand() : base("get_lightning", "gl",
            "Prints when lightning will harm terrain features today.")
        {
        }

        public override string Parse(string[] args)
        {
            string result = "Okay, lightning strikes today will be at:";
            for (int i = 600; i < 2400; i = SafeLightningMod.GetNextTime(i))
                if (SDVLightningMimic.GetSDVLightningStrikePositionAt(new LightningStrikeRNGInfo(i),
                    out KeyValuePair<Vector2, TerrainFeature> feature))
                    result +=
                        $"\nLightning strike: {i} at {feature.Key} on {feature.Value.GetType().Name}.";
            return result;
        }
    }
}