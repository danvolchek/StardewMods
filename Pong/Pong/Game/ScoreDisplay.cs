using System;
using Microsoft.Xna.Framework.Graphics;
using Pong.Game.Interfaces;
using StardewValley.BellsAndWhistles;

namespace Pong.Game
{
    class ScoreDisplay : IUpdateable, IResetable
    {
        public int playerScore;
        public int computerScore;

        public ScoreDisplay()
        {
            Reset();
        }

        public void UpdateScore(bool playerWon)
        {
            if (playerWon)
                playerScore++;
            else
                computerScore++;
        }

        public void Draw(SpriteBatch b)
        {
            SpriteText.drawString(b, $"{playerScore} - {computerScore}", 50, 50, 999999, -1, 999999, 1f, 0.88f, false, -1, "", SpriteText.color_White);
        }

        public void Reset()
        {
            playerScore = computerScore = 0;
        }

        public void Update()
        {
        }
    }
}
