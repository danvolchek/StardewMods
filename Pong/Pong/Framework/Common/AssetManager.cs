using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace Pong.Framework.Common
{
    internal static class AssetManager
    {
        public static Texture2D SquareTexture;
        public static Texture2D CircleTexture;
        public static Texture2D BorderTexture;

        public static bool Init(IModHelper helper)
        {
            try
            {
                SquareTexture = helper.Content.Load<Texture2D>("assets/square.png");
                CircleTexture = helper.Content.Load<Texture2D>("assets/circle.png");
                BorderTexture = helper.Content.Load<Texture2D>("assets/border.png");

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
