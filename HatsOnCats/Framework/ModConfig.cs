using System.Collections.Generic;
using HatsOnCats.Framework.Configuration;
using Microsoft.Xna.Framework;

namespace HatsOnCats.Framework
{
    internal class ModConfig
    {
        public IDictionary<string, IDictionary<Frame, Offset>> Offsets { get; set; } = new Dictionary<string, IDictionary<Frame, Offset>>();
    }
}
