using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RangeDisplay.Framework.APIs
{
    /// <summary>The prismatic tools API.</summary>
    public interface IPrismaticToolsAPI
    {
        /*********
        ** Accessors
        *********/

        /// <summary>The parentSheetIndex of prismatic sprinklers.</summary>
        int SprinklerIndex { get; }

        /// <summary>Whether prismatic sprinklers also act as scarecrows.</summary>
        bool ArePrismaticSprinklersScarecrows { get; }

        /*********
        ** Methods
        *********/

        /// <summary>Gets the coverage of a prismatic sprinkler.</summary>
        /// <param name="origin">The position of the prismatic sprinkler.</param>
        /// <returns>The sprinkler coverage.</returns>
        IEnumerable<Vector2> GetSprinklerCoverage(Vector2 origin);
    }
}
