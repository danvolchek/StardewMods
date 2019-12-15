using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace HatsOnCats.Framework.Configuration
{
    [JsonConverter(typeof(OffsetConverter))]
    internal struct Offset
    {
        public Vector2 Value { get; }
        public OffsetReference Reference { get; }
        public bool IsReference { get; }

        public Offset(Vector2 value) : this(value, OffsetReference.Zero, false)
        {
        }

        public Offset(OffsetReference reference) : this(Vector2.Zero, reference, true)
        {
        }

        private Offset(Vector2 value, OffsetReference reference, bool isReference)
        {
            this.Value = value;
            this.Reference = reference;
            this.IsReference = isReference;
        }
    }
}
