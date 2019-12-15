using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HatsOnCats.Framework.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace HatsOnCats.Framework.Configuration
{
    [TypeConverter(typeof(FromStringConverter<Frame>))]
    internal struct Frame
    {
        public Point Point { get; }
        public SpriteEffects Effects { get; }

        public Frame(Point point, SpriteEffects effects = SpriteEffects.None)
        {
            this.Point = point;
            this.Effects = effects;
        }

        public static Frame Zero = new Frame(Point.Zero);

        public override string ToString()
        {
            string value = $"{this.Point.X}, {this.Point.Y}";

            switch (this.Effects)
            {
                
                case SpriteEffects.FlipHorizontally:
                    return $"h, {value}";
                case SpriteEffects.FlipVertically:
                    return $"v, {value}";
                default:
                case SpriteEffects.None:
                    return value;
            }
        }

        public static Frame FromString(string str)
        {
            string[] parts = str.Split(',');

            if (parts.Length == 2)
            {
                return new Frame(new Point(int.Parse(parts[0].Trim()), int.Parse(parts[1].Trim())));
            }

            string flipIndicator = parts[0].Trim().ToLower();

            SpriteEffects effects = flipIndicator == "h" ? SpriteEffects.FlipHorizontally : (flipIndicator == "v" ? SpriteEffects.FlipVertically : throw new JsonReaderException($"Frame flip indicator must be either x or y, not '{parts[0]}'"));

            return new Frame(new Point(int.Parse(parts[1].Trim()), int.Parse(parts[2].Trim())), effects);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Frame other))
            {
                return false;
            }

            return this.Point.Equals(other.Point) && this.Effects == other.Effects;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Point.GetHashCode() * 397) ^ (int) this.Effects;
            }
        }
    }
}
