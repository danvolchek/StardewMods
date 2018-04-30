using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RangeDisplay.APIs
{
    /// <summary>API for simple sprinklers.</summary>
    public interface ISimpleSprinklersAPI
    {
        IDictionary<int, Vector2[]> GetNewSprinklerCoverage();
    }
}
