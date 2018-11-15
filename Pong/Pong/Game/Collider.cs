using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Common;
using Pong.Framework.Game;
using Pong.Framework.Game.States;
using StardewModdingAPI;

namespace Pong.Game
{
    internal abstract class Collider : IDrawableCollideable
    {
        protected PositionState PositionState;

        protected int XPos
        {
            get => this.PositionState.XPosition;
            set => this.PositionState.XPosition = value;
        }

        protected int YPos
        {
            get => this.PositionState.YPosition;
            set => this.PositionState.YPosition = value;
        }

        protected int Width;
        protected int Height;
        protected bool IsSquare;

        public Rectangle Bounds { get; protected set; }

        protected Collider(bool isSquare) : this(new PositionState(), isSquare)
        {
        }

        protected Collider(PositionState state, bool isSquare)
        {
            this.PositionState = state;
            this.IsSquare = isSquare;
            this.UpdateBounds();
            this.PositionState.StateChanged += this.PositionState_StateChanged;
        }

        private void PositionState_StateChanged(object sender, System.EventArgs e)
        {
            this.UpdateBounds();
        }

        private void UpdateBounds()
        {
            this.Bounds = new Rectangle(this.XPos, this.YPos, this.Width, this.Height);
        }

        public virtual void Draw(SpriteBatch b)
        {
            b.Draw(this.IsSquare ? AssetManager.SquareTexture : AssetManager.CircleTexture, this.Bounds, null, Color.White);
        }

        public virtual void Update()
        {
        }
    }
}
