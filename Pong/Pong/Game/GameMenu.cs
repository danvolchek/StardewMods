using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Enums;
using Pong.Framework.Interfaces.Game;
using Pong.Framework.Menus;
using Pong.Framework.Menus.Elements;
using Pong.Game.Controllers;
using Pong.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using IDrawable = Pong.Framework.Common.IDrawable;

namespace Pong.Game
{
    internal class GameMenu : Menu, IResetable
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

        public GameMenu()
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

            this.InitDrawables();
        }

        public override bool ButtonPressed(EventArgsInput e)
        {
            switch (e.Button)
            {
                case SButton.P:
                    this.TogglePaused();
                    return true;
                case SButton.Escape:
                    this.OnSwitchToNewMenu(new StartScreen());
                    return true;
            }

            return false;
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

        protected override IEnumerable<IDrawable> GetDrawables()
        {
            foreach (INonReactiveDrawableCollideable collideable in this.nonReactiveCollideables)
                yield return collideable;

            yield return this.ball;
            yield return this.scoreDisplay;

            yield return new ConditionalElement(new StaticTextElement("Press P to resume", ScreenWidth / 2, ScreenHeight /2),
                () => this.paused);

            yield return new ConditionalElement(new DynamicTextElement(() => $"{this.startTimer / 60 + 1}", ScreenWidth / 2, ScreenHeight / 2), 
                () => !this.paused && this.starting);

            yield return new ConditionalElement(new StaticTextElement("P to pause", 50, 150),
                () => !this.paused && !this.starting);

            yield return new ConditionalElement(new StaticTextElement("Esc to exit", 50, 100),
                () => !this.starting);
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