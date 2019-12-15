using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RangeDisplay.Framework.APIs
{
    /// <summary>The better sprinklers API.</summary>
    public interface IBetterSprinklersAPI
    {
        /*********
        ** Methods
        *********/

        /// <summary>Gets the coverage of sprinklers by parentSheetIndex.</summary>
        /// <returns>The sprinkler coverage.</returns>
        IDictionary<int, Vector2[]> GetSprinklerCoverage();
    }
}
