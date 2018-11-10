using System;
using Microsoft.Xna.Framework.Graphics;
using Pong.Game.Interfaces;
using StardewValley.BellsAndWhistles;

namespace Pong.Game
{
    internal class ScoreDisplay : IUpdateable, IResetable
    {
        public int PlayerScore { get; private set; }
        public int ComputerScore { get; private set; }

        public ScoreDisplay()
        {
            this.Reset();
        }

        public void UpdateScore(bool playerWon)
        {
            if (playerWon)
                this.PlayerScore++;
            else
                this.ComputerScore++;
        }

        public void Draw(SpriteBatch b)
        {
            SpriteText.drawString(b, $"{this.PlayerScore} - {this.ComputerScore}", 50, 50, 999999, -1, 999999, 1f, 0.88f, false, -1, "", SpriteText.color_White);
        }

        public void Reset()
        {
            this.PlayerScore = this.ComputerScore = 0;
        }

        public void Update()
        {
        }
    }
}
