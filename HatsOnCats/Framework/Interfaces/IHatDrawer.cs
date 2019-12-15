using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using System.Collections.Generic;
using StardewValley;

namespace HatsOnCats.Framework.Interfaces
{
    internal interface IHatDrawer
    {
        void DrawHats(IEnumerable<Hat> hats, int facingDirection, SpriteBatch spriteBatch, Vector2 position, Vector2 origin, float rotation, float layerDepth);
    }
}
