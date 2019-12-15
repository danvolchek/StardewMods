using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace HatsOnCats.Framework
{
    internal static class AnimatedSpriteExtensions
    {
        public static string UniqueName(this AnimatedSprite sprite)
        {
            return sprite.textureName.Value.Split('\\').Last().ToLower();
        }
    }
}
