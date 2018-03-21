using StardewValley;

namespace Pong
{
    class SoundManager
    {
        public void PlayBallWallSound()
        {
            Game1.playSound("smallSelect");
        }

        public void PlayBallPaddleSound()
        {
            Game1.playSound("smallSelect");
        }

        public void PlayPointWonSound(bool playerWon)
        {
            Game1.playSound(playerWon ? "achievement" : "trashcan");
        }

        public void PlayCountdownSound()
        {
            Game1.playSound("throwDownITem");
        }

        public void PlayKeyPressSound()
        {
            Game1.playSound("bigDeSelect");
        }
    }
}
