using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Enums;
using Pong.Framework.Interfaces;
using Pong.Framework.Menus;
using Pong.Game.Controllers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace Pong.Game
{
    internal class PongGame : Menu, IResetable
    {
        public static Texture2D SquareTexture;
        public static Texture2D CircleTexture;
        private readonly List<INonReactiveDrawableCollideable> nonReactiveCollideables;
        private readonly List<IResetable> resetables;
        private readonly ScoreDisplay scoreDisplay;

        private readonly Ball ball;
        private bool ballCollidedLastFrame;
        private bool starting;
        private int startTimer;
        private bool paused;

        public PongGame()
        {
            this.ball = new Ball();
            Paddle computerPaddle = new Paddle(new ComputerController(this.ball), Side.Top);
            Paddle playerPaddle = new Paddle(new LocalPlayerController(), Side.Bottom);
            this.scoreDisplay = new ScoreDisplay();

            this.nonReactiveCollideables = new List<INonReactiveDrawableCollideable>
            {
                playerPaddle,
                computerPaddle,
                new Wall(Side.Bottom),
                new Wall(Side.Left),
                new Wall(Side.Right),
                new Wall(Side.Top)
            };

            this.resetables = new List<IResetable>
            {
                this.ball,
                this.scoreDisplay,
                playerPaddle,
                computerPaddle
            };

            this.ballCollidedLastFrame = false;
            this.starting = true;
            this.startTimer = 180;
            this.paused = false;
        }

        public override void ButtonPressed(EventArgsInput e)
        {
            e.SuppressButton();
            switch (e.Button)
            {
                case SButton.P:
                    this.TogglePaused();
                    break;
                case SButton.Escape:
                    this.OnSwitchToNewMenu(new StartScreen());
                    break;
            }
        }


        public override void Update()
        {
            if (!this.starting && !this.paused)
            {
                bool collided = false;
                foreach (INonReactiveDrawableCollideable collideable in this.nonReactiveCollideables)
                {
                    if (this.ball.GetBoundingBox().Intersects(collideable.GetBoundingBox()))
                    {
                        this.ball.CollideWith(collideable);

                        if (collideable is Wall wall)
                        {
                            if (wall.Side == Side.Bottom || wall.Side == Side.Top)
                            {
                                SoundManager.PlayPointWonSound(wall.Side == Side.Top);
                                this.scoreDisplay.UpdateScore(wall.Side == Side.Top);
                                this.Reset(false);
                                this.Start();
                                return;
                            }
                            else
                            {
                                SoundManager.PlayBallWallSound();
                            }
                        }
                        else if (collideable is Paddle)
                        {
                            SoundManager.PlayBallPaddleSound();
                        }

                        collided = true;
                    }

                    collideable.Update();
                }

                if (collided && this.ballCollidedLastFrame) this.ball.Reset();

                this.ballCollidedLastFrame = collided;

                this.ball.Update();
                this.scoreDisplay.Update();
            }
            else if (this.starting)
            {
                if (this.startTimer % 60 == 0)
                    SoundManager.PlayCountdownSound();
                this.startTimer--;
                if (this.startTimer == 0)
                {
                    this.starting = false;
                }
            }
        }

        public override void Draw(SpriteBatch b)
        {
            base.Draw(b);

            foreach (INonReactiveDrawableCollideable collideable in this.nonReactiveCollideables)
                collideable.Draw(b);

            this.ball.Draw(b);
            this.scoreDisplay.Draw(b);

            if (this.paused)
                SpriteText.drawStringHorizontallyCenteredAt(b, "Press P to resume", ScreenWidth / 2,
                    ScreenHeight / 2, 999999, -1, 999999, 1f, 0.88f, false, SpriteText.color_White);
            else if (this.starting)
                SpriteText.drawStringHorizontallyCenteredAt(b,
                    $"{this.startTimer / 60 + 1}", ScreenWidth / 2,
                    (int)(ScreenHeight / 2.0 - Game1.tileSize * 1.5), 999999, -1, 999999, 1f, 0.88f, false,
                    SpriteText.color_White);
            else
                SpriteText.drawString(b, "P to pause", 50, 150, 999999, -1, 999999, 1f, 0.88f, false, -1, "",
                    SpriteText.color_White);

            if (!this.starting)
                SpriteText.drawString(b, "Esc to exit", 50, 100, 999999, -1, 999999, 1f, 0.88f, false, -1, "",
                    SpriteText.color_White);


            b.Draw(Game1.mouseCursors,
                new Rectangle(Game1.oldMouseState.X, Game1.oldMouseState.Y, Game1.tileSize / 2, Game1.tileSize / 2),
                new Rectangle(146, 384, 9, 9), Color.White);
        }

        private void Reset(bool resetScore)
        {
            if (this.starting)
                return;

            this.ballCollidedLastFrame = false;
            this.starting = true;
            this.startTimer = 180;
            this.paused = false;

            foreach (IResetable resetable in this.resetables)
                if (resetable != this.scoreDisplay || resetable == this.scoreDisplay && resetScore)
                    resetable.Reset();
        }

        public void Reset()
        {
            this.Reset(true);
            SoundManager.PlayKeyPressSound();
        }

        public override void Resize()
        {
            foreach (INonReactiveDrawableCollideable collideable in this.nonReactiveCollideables)
                collideable.Resize();
        }

        private void Start()
        {
            if (this.starting)
                return;

            this.starting = true;
        }

        private void TogglePaused()
        {
            if (this.starting)
                return;

            SoundManager.PlayKeyPressSound();
            this.paused = !this.paused;
        }

    }
}