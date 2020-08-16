using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace WindEffects.Framework.Shakers
{
    internal interface IShaker
    {
        void Shake(IReflectionHelper helper, Vector2 tile);
    }
}
