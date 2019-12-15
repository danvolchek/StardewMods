using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BetterGardenPots.APIs
{
    /// <summary>API for prismatic tools.</summary>
    public interface IPrismaticToolsAPI
    {
        int SprinklerIndex { get; }

        IEnumerable<Vector2> GetSprinklerCoverage(Vector2 origin);
    }
}
