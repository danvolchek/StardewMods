using HatsOnCats.Framework.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace HatsOnCats.Framework.Offsets
{
    internal interface IOffsetProvider : INamed
    {
        bool GetOffset(Rectangle sourceRectangle, SpriteEffects effects, out Vector2 offset);
        bool CanHandle(string spriteName);
    }
}
