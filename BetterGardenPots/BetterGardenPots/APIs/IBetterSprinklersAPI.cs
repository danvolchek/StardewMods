using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BetterGardenPots.APIs
{
    /// <summary>API for better sprinklers.</summary>
    internal interface IBetterSprinklersAPI
    {
        IDictionary<int, Vector2[]> GetSprinklerCoverage();
    }
}