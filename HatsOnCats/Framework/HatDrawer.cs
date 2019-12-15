using HatsOnCats.Framework.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

namespace HatsOnCats.Framework
{
    internal class HatDrawer : IHatDrawer
    {
        public void DrawHats(IEnumerable<Hat> hats, int facingDirection, SpriteBatch spriteBatch, Vector2 position, Vector2 origin, float rotation, float layerDepth)
        {
            foreach (Hat hat in hats)
            {
                this.DrawHat(hat, spriteBatch, position, 1.333f, 1, layerDepth + 0.001f, origin, rotation, facingDirection);
            }
        }

        private void DrawHat(Hat hat,
            SpriteBatch spriteBatch,
            Vector2 location,
            float scaleSize,
            float transparency,
            float layerDepth,
            Vector2 origin,
            float rotation,
            int direction)
        {
            // From a drawing perspective:
            // 0 - down
            // 1 - right
            // 2 - left
            // 3 - up
            /*switch (direction)
            {
                case 0:
                    direction = 3;
                    break;
                case 2:
                    direction = 0;
                    break;
                case 3:
                    direction = 2;
                    break;
            }*/
            spriteBatch.Draw(FarmerRenderer.hatsTexture, location, new Rectangle?(new Rectangle(hat.which.Value * 20 % FarmerRenderer.hatsTexture.Width, hat.which.Value * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4 + direction * 20, 20, 20)), hat.isPrismatic.Value ? Utility.GetPrismaticColor(0) * transparency : Color.White * transparency, rotation, origin, 3f * scaleSize, SpriteEffects.None, layerDepth);
        }
    }
}
