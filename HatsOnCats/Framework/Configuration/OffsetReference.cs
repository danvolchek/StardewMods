using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace HatsOnCats.Framework.Configuration
{
    [JsonConverter(typeof(OffsetReferenceConverter))]
    internal struct OffsetReference
    {
        public Frame Frame { get; }

        public OffsetReference(Frame frame)
        {
            this.Frame = frame;
        }

        public static OffsetReference Zero = new OffsetReference(Frame.Zero);

        public override string ToString()
        {
            return $"!{this.Frame.ToString()}";
        }
    }
}
