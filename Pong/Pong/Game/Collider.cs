using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pong.Game
{
    abstract class Collider : ICollideable
    {
        protected int xPos;
        protected int yPos;
        protected int width;
        protected int height;

        protected bool isSquare;

        public Collider(bool isSquare)
        {
            this.isSquare = isSquare;
        }

        public Rectangle GetBoundingBox()
        {
            return new Rectangle(xPos, yPos, width, height);
        }

        public virtual void Draw(SpriteBatch b)
        {
            b.Draw(isSquare ? PongGame.squareTexture : PongGame.circleTexture, new Rectangle(xPos, yPos, width, height), null, Color.White);
        }

        public virtual void Update()
        {

        }
    }
}
