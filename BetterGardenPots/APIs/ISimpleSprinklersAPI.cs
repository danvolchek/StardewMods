using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BetterGardenPots.APIs
{
    /// <summary>API for simple sprinklers.</summary>
    public interface ISimpleSprinklersAPI
    {
        IDictionary<int, Vector2[]> GetNewSprinklerCoverage();
    }
}
