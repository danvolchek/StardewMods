using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Game;
using Pong.Game.Interfaces;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System.Collections.Generic;

namespace Pong
{
    class PongGame : Game.IUpdateable, IResetable
    {
        public enum Side { LEFT, TOP, RIGHT, BOTTOM };
        public enum Orientation { HORIZONTAL, VERTICAL };

        public static Texture2D squareTexture;
        public static Texture2D circleTexture;
        private List<INonReactiveCollideable> nonReactiveCollideables;
        private List<IResetable> resetables;
        private Paddle playerPaddle;
        private Paddle computerPaddle;
        private Ball ball;
        private ScoreDisplay scoreDisplay;

        private SoundManager soundManager;

        private bool ballCollidedLastFrame;
        private bool starting;
        private int startTimer;
        private bool started;
        private bool paused;
        private int lastMouseXPos;

        public PongGame(IModHelper helper)
        {
            squareTexture = helper.Content.Load<Texture2D>("assets/square.png");
            circleTexture = helper.Content.Load<Texture2D>("assets/circle.png");

            computerPaddle = new Paddle(false);
            playerPaddle = new Paddle(true);
            ball = new Ball();
            scoreDisplay = new ScoreDisplay();

            soundManager = new SoundManager();

            nonReactiveCollideables = new List<INonReactiveCollideable>
            {
                playerPaddle,
                computerPaddle,
                new Wall(Side.BOTTOM),
                new Wall(Side.LEFT),
                new Wall(Side.RIGHT),
                new Wall(Side.TOP)
            };

            resetables = new List<IResetable>
            {
                ball,
                scoreDisplay,
                playerPaddle,
                computerPaddle
            };

            ballCollidedLastFrame = false;
            started = false;
            starting = false;
            startTimer = 180;
            paused = false;
            lastMouseXPos = 0;
        }


        public void Update()
        {
            if (started && !paused)
            {
                bool collided = false;
                foreach (INonReactiveCollideable collideable in nonReactiveCollideables)
                {
                    if (ball.GetBoundingBox().Intersects(collideable.GetBoundingBox()))
                    {
                        ball.CollideWith(collideable);

                        if (collideable is Wall wall)
                        {
                            if (wall.side == Side.BOTTOM || wall.side == Side.TOP)
                            {
                                soundManager.PlayPointWonSound(wall.side == Side.TOP);
                                scoreDisplay.UpdateScore(wall.side == Side.TOP);
                                Reset(false);
                                Start();
                                return;
                            }
                            else
                            {
                                soundManager.PlayBallWallSound();
                            }
                        }
                        else if (collideable is Paddle)
                        {
                            soundManager.PlayBallPaddleSound();
                        }

                        collided = true;
                    }

                    if (collideable == computerPaddle)
                        computerPaddle.ReceiveIntendedPosition(ball.GetBoundingBox().X + ball.GetBoundingBox().Width / 2);
                    else if (collideable == playerPaddle)
                        playerPaddle.ReceiveIntendedPosition(lastMouseXPos);

                    collideable.Update();
                }

                if (collided && ballCollidedLastFrame)
                    ball.Reset();

                ballCollidedLastFrame = collided;

                ball.Update();
                scoreDisplay.Update();
            }
            else if (starting)
            {
                startTimer--;
                if (startTimer == 60 || startTimer == 120 || startTimer == 0)
                    soundManager.PlayCountdownSound();
                if (startTimer == 0)
                {
                    started = true;
                    starting = false;
                }
            }
        }

        public void Draw(SpriteBatch b)
        {
            b.Draw(squareTexture, new Rectangle(0, 0, GetScreenWidth(), GetScreenWidth()), null, Color.Black);
            if (started || starting)
            {
                foreach (INonReactiveCollideable collideable in nonReactiveCollideables)
                {
                    collideable.Draw(b);
                }
                ball.Draw(b);
                scoreDisplay.Draw(b);

                if (paused)
                {
                    SpriteText.drawStringHorizontallyCenteredAt(b, "Press P to resume", GetScreenWidth() / 2, GetScreenHeight() / 2, 999999, -1, 999999, 1f, 0.88f, false, SpriteText.color_White);
                }
                else if (starting)
                {
                    SpriteText.drawStringHorizontallyCenteredAt(b, $"{(startTimer < 60 ? 1 : (startTimer < 120 ? 2 : 3))}", GetScreenWidth() / 2, (int)(GetScreenHeight() / 2 - Game1.tileSize * 1.5), 999999, -1, 999999, 1f, 0.88f, false, SpriteText.color_White);
                }
                else
                {
                    SpriteText.drawString(b, "P to pause", 50, 150, 999999, -1, 999999, 1f, 0.88f, false, -1, "", SpriteText.color_White);
                }

                if (!starting)
                {
                    SpriteText.drawString(b, "Esc to exit", 50, 100, 999999, -1, 999999, 1f, 0.88f, false, -1, "", SpriteText.color_White);
                }
            }
            else
            {
                int centerHeight = SpriteText.getHeightOfString("Press Space to start");
                SpriteText.drawStringHorizontallyCenteredAt(b, "Pong", GetScreenWidth() / 2, GetScreenHeight() / 2 - centerHeight * 5, 999999, -1, 999999, 1f, 0.88f, false, SpriteText.color_White);
                SpriteText.drawStringHorizontallyCenteredAt(b, "By Cat", GetScreenWidth() / 2, GetScreenHeight() / 2 - centerHeight * 4, 999999, -1, 999999, 1f, 0.88f, false, SpriteText.color_White);

                SpriteText.drawStringHorizontallyCenteredAt(b, "Press Space to start", GetScreenWidth() / 2, GetScreenHeight() / 2, 999999, -1, 999999, 1f, 0.88f, false, SpriteText.color_White);
                int escHeight = SpriteText.getHeightOfString("Press Esc to exit");
                SpriteText.drawString(b, "Press Esc to exit", 0, GetScreenHeight() - escHeight, 999999, -1, 999999, 1f, 0.88f, false, -1, "", SpriteText.color_White);
            }
            b.Draw(Game1.mouseCursors, new Rectangle(Game1.oldMouseState.X, Game1.oldMouseState.Y, Game1.tileSize / 2, Game1.tileSize / 2), new Rectangle(146, 384, 9, 9), Color.White);
        }

        private void Reset(bool resetScore)
        {
            if (!started)
                return;

            ballCollidedLastFrame = false;
            started = false;
            starting = false;
            startTimer = 180;
            paused = false;
            lastMouseXPos = 0;
            foreach (IResetable resetable in resetables)
            {
                if (resetable != scoreDisplay || (resetable == scoreDisplay && resetScore))
                    resetable.Reset();
            }
        }

        public void Reset()
        {
            Reset(true);
            soundManager.PlayKeyPressSound();
        }

        public void Resize()
        {
            foreach (INonReactiveCollideable collideable in nonReactiveCollideables)
            {
                collideable.Resize();
            }
        }

        public void Start()
        {
            if (starting || started)
                return;

            soundManager.PlayKeyPressSound();
            starting = true;
        }

        public void TogglePaused()
        {
            if (!started)
                return;

            soundManager.PlayKeyPressSound();
            paused = !paused;
        }

        public bool HasStarted()
        {
            return starting || started;
        }

        public void MouseChanged(Point p)
        {
            lastMouseXPos = p.X;
        }

        public static int GetScreenWidth()
        {
            return Game1.graphics.GraphicsDevice.Viewport.Width;
        }

        public static int GetScreenHeight()
        {
            return Game1.graphics.GraphicsDevice.Viewport.Height;
        }
    }
}
