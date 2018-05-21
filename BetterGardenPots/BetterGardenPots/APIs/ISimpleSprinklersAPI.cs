using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BetterGardenPots.APIs
{
    /// <summary>API for simple sprinklers.</summary>
    public interface ISimpleSprinklersAPI
    {
        IDictionary<int, Vector2[]> GetNewSprinklerCoverage();
    }
}
