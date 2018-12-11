using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RangeDisplay.APIs
{
    public interface IPrismaticToolsAPI
    {
        int SprinklerIndex { get; }
        bool ArePrismaticSprinklersScarecrows { get; }
        IEnumerable<Vector2> GetSprinklerCoverage(Vector2 origin);
    }
}
