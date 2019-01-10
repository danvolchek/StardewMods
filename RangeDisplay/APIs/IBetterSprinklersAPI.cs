using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RangeDisplay.APIs
{
    /// <summary>API for better sprinklers.</summary>
    public interface IBetterSprinklersAPI
    {
        IDictionary<int, Vector2[]> GetSprinklerCoverage();
    }
}
