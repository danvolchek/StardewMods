using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;

namespace WindEffects.Framework
{
    internal static class SpriteBatchExtensions
    {
        private static Texture2D pixel;

        public static void Init(IModHelper helper)
        {
            SpriteBatchExtensions.pixel = helper.Content.Load<Texture2D>("assets/pixel.png");
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, int lineWidth)
        {

            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            spriteBatch.Draw(pixel, point1, null, color, angle, Vector2.Zero, new Vector2(length, lineWidth), SpriteEffects.None, 0f);
        }
    }
}
