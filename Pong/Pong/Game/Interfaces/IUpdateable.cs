using Microsoft.Xna.Framework.Graphics;

namespace Pong.Game.Interfaces
{
    interface IUpdateable
    {
        void Update();
        void Draw(SpriteBatch b);
    }
}
