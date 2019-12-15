using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RangeDisplay.Framework.APIs
{
    /// <summary>The simple sprinklers API.</summary>
    public interface ISimpleSprinklersAPI
    {
        /// <summary>Get the extra range covered by sprinklers, on top of the vanilla range.</summary>
        /// <returns>The extra range.</returns>
        IDictionary<int, Vector2[]> GetNewSprinklerCoverage();
    }
}
