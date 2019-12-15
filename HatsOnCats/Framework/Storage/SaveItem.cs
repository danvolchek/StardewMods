using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace HatsOnCats.Framework.Storage
{
    [TypeConverter(typeof(FromStringConverter<SaveItem>))]
    internal struct SaveItem
    {
        public string Location { get; set; }
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public string Type { get; set; }

        public SaveItem(string location, string type, string name, Vector2 position)
        {
            this.Location = location;
            this.Type = type;
            this.Name = name;
            this.Position = position;
        }

        public override string ToString()
        {
            return $"{this.Location}, {this.Type}, {this.Name}, {this.Position.X}, {this.Position.Y}";
        }

        public static SaveItem FromString(string str)
        {
            string[] parts = str.Split(',');

            return new SaveItem(parts[0].Trim(), parts[1].Trim(), parts[2].Trim(), new Vector2(float.Parse(parts[3].Trim()), float.Parse(parts[4].Trim())));
        }
    }
}
