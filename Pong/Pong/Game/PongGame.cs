using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Game.Interfaces;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using IUpdateable = Pong.Game.Interfaces.IUpdateable;

namespace Pong.Game
{
    internal class PongGame : IUpdateable, IResetable
    {
        public enum Side
        {
            Left,
            Top,
            Right,
            Bottom
        };

        public enum Orientation
        {
            Horizontal,
            Vertical
        };

        public static Texture2D SquareTexture;
        public static Texture2D CircleTexture;
        private readonly List<INonReactiveCollideable> nonReactiveCollideables;
        private readonly List<IResetable> resetables;
        private readonly Paddle playerPaddle;
        private readonly Paddle computerPaddle;
        private readonly Ball ball;
        private readonly ScoreDisplay scoreDisplay;

        private readonly SoundManager soundManager;

        private bool ballCollidedLastFrame;
        private bool starting;
        private int startTimer;
        private bool started;
        private bool paused;
        private int lastMouseXPos;

        public PongGame(IModHelper helper)
        {
            SquareTexture = helper.Content.Load<Texture2D>("assets/square.png");
            CircleTexture = helper.Content.Load<Texture2D>("assets/circle.png");

            this.computerPaddle = new Paddle(false);
            this.playerPaddle = new Paddle(true);
            this.ball = new Ball();
            this.scoreDisplay = new ScoreDisplay();

            this.soundManager = new SoundManager();

            this.nonReactiveCollideables = new List<INonReactiveCollideable>
            {
                this.playerPaddle,
                this.computerPaddle,
                new Wall(Side.Bottom),
                new Wall(Side.Left),
                new Wall(Side.Right),
                new Wall(Side.Top)
            };

            this.resetables = new List<IResetable>
            {
                this.ball,
                this.scoreDisplay,
                this.playerPaddle,
                this.computerPaddle
            };

            this.ballCollidedLastFrame = false;
            this.started = false;
            this.starting = false;
            this.startTimer = 180;
            this.paused = false;
            this.lastMouseXPos = 0;
        }


        public void Update()
        {
            if (this.started && !this.paused)
            {
                bool collided = false;
                foreach (INonReactiveCollideable collideable in this.nonReactiveCollideables)
                {
                    if (this.ball.GetBoundingBox().Intersects(collideable.GetBoundingBox()))
                    {
                        this.ball.CollideWith(collideable);

                        if (collideable is Wall wall)
                        {
                            if (wall.Side == Side.Bottom || wall.Side == Side.Top)
                            {
                                this.soundManager.PlayPointWonSound(wall.Side == Side.Top);
                                this.scoreDisplay.UpdateScore(wall.Side == Side.Top);
                                this.Reset(false);
                                this.Start();
                                return;
                            }
                            else
                            {
                                this.soundManager.PlayBallWallSound();
                            }
                        }
                        else if (collideable is Paddle)
                        {
                            this.soundManager.PlayBallPaddleSound();
                        }

                        collided = true;
                    }

                    if (collideable == this.computerPaddle)
                        this.computerPaddle.ReceiveIntendedPosition(this.ball.GetBoundingBox().X + this.ball.GetBoundingBox().Width / 2);
                    else if (collideable == this.playerPaddle) this.playerPaddle.ReceiveIntendedPosition(this.lastMouseXPos);

                    collideable.Update();
                }

                if (collided && this.ballCollidedLastFrame) this.ball.Reset();

                this.ballCollidedLastFrame = collided;

                this.ball.Update();
                this.scoreDisplay.Update();
            }
            else if (this.starting)
            {
                this.startTimer--;
                if (this.startTimer == 60 || this.startTimer == 120 || this.startTimer == 0) this.soundManager.PlayCountdownSound();
                if (this.startTimer == 0)
                {
                    this.started = true;
                    this.starting = false;
                }
            }
        }

        public void Draw(SpriteBatch b)
        {
            b.Draw(SquareTexture, new Rectangle(0, 0, GetScreenWidth(), GetScreenWidth()), null, Color.Black);
            if (this.started || this.starting)
            {
                foreach (INonReactiveCollideable collideable in this.nonReactiveCollideables) collideable.Draw(b);

                this.ball.Draw(b);
                this.scoreDisplay.Draw(b);

                if (this.paused)
                    SpriteText.drawStringHorizontallyCenteredAt(b, "Press P to resume", GetScreenWidth() / 2,
                        GetScreenHeight() / 2, 999999, -1, 999999, 1f, 0.88f, false, SpriteText.color_White);
                else if (this.starting)
                    SpriteText.drawStringHorizontallyCenteredAt(b,
                        $"{(this.startTimer < 60 ? 1 : (this.startTimer < 120 ? 2 : 3))}", GetScreenWidth() / 2,
                        (int) (GetScreenHeight() / 2 - Game1.tileSize * 1.5), 999999, -1, 999999, 1f, 0.88f, false,
                        SpriteText.color_White);
                else
                    SpriteText.drawString(b, "P to pause", 50, 150, 999999, -1, 999999, 1f, 0.88f, false, -1, "",
                        SpriteText.color_White);

                if (!this.starting)
                    SpriteText.drawString(b, "Esc to exit", 50, 100, 999999, -1, 999999, 1f, 0.88f, false, -1, "",
                        SpriteText.color_White);
            }
            else
            {
                int centerHeight = SpriteText.getHeightOfString("Press Space to start");
                SpriteText.drawStringHorizontallyCenteredAt(b, "Pong", GetScreenWidth() / 2,
                    GetScreenHeight() / 2 - centerHeight * 5, 999999, -1, 999999, 1f, 0.88f, false,
                    SpriteText.color_White);
                SpriteText.drawStringHorizontallyCenteredAt(b, "By Cat", GetScreenWidth() / 2,
                    GetScreenHeight() / 2 - centerHeight * 4, 999999, -1, 999999, 1f, 0.88f, false,
                    SpriteText.color_White);

                SpriteText.drawStringHorizontallyCenteredAt(b, "Press Space to start", GetScreenWidth() / 2,
                    GetScreenHeight() / 2, 999999, -1, 999999, 1f, 0.88f, false, SpriteText.color_White);
                int escHeight = SpriteText.getHeightOfString("Press Esc to exit");
                SpriteText.drawString(b, "Press Esc to exit", 0, GetScreenHeight() - escHeight, 999999, -1, 999999, 1f,
                    0.88f, false, -1, "", SpriteText.color_White);
            }

            b.Draw(Game1.mouseCursors,
                new Rectangle(Game1.oldMouseState.X, Game1.oldMouseState.Y, Game1.tileSize / 2, Game1.tileSize / 2),
                new Rectangle(146, 384, 9, 9), Color.White);
        }

        private void Reset(bool resetScore)
        {
            if (!this.started)
                return;

            this.ballCollidedLastFrame = false;
            this.started = false;
            this.starting = false;
            this.startTimer = 180;
            this.paused = false;
            this.lastMouseXPos = 0;
            foreach (IResetable resetable in this.resetables)
                if (resetable != this.scoreDisplay || resetable == this.scoreDisplay && resetScore)
                    resetable.Reset();
        }

        public void Reset()
        {
            this.Reset(true);
            this.soundManager.PlayKeyPressSound();
        }

        public void Resize()
        {
            foreach (INonReactiveCollideable collideable in this.nonReactiveCollideables) collideable.Resize();
        }

        public void Start()
        {
            if (this.starting || this.started)
                return;

            this.soundManager.PlayKeyPressSound();
            this.starting = true;
        }

        public void TogglePaused()
        {
            if (!this.started)
                return;

            this.soundManager.PlayKeyPressSound();
            this.paused = !this.paused;
        }

        public bool HasStarted()
        {
            return this.starting || this.started;
        }

        public void MouseChanged(Point p)
        {
            this.lastMouseXPos = p.X;
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