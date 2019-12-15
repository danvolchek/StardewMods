using System.Collections.Generic;
using HatsOnCats.Framework.Interfaces;
using Microsoft.Xna.Framework;

namespace HatsOnCats.Framework.Configuration
{
    internal interface IConfigurable : INamed
    {
        IDictionary<Frame, Offset> Configuration { get; set; }
    }
}
