using Pong.Framework.Common;
using Pong.Framework.Enums;
using Pong.Framework.Game;
using Pong.Framework.Menus;
using Pong.Framework.Menus.Elements;
using Pong.Game;
using Pong.Game.Controllers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using Pong.Framework.Game.States;
using IDrawable = Pong.Framework.Common.IDrawable;

namespace Pong.Menus
{
    internal class GameMenu : Menu, IResetable
    {
        private readonly List<INonReactiveDrawableCollideable> nonReactiveCollideables;
        private readonly List<IResetable> resetables;
        private readonly ScoreDisplay scoreDisplay;

        private readonly Ball ball;
        private readonly GameState state = new GameState();

        public GameMenu(long? enemyPlayer = null)
        {
            this.ball = new Ball(this.state.PositionState, this.state.VelocityState);
            Paddle computerPaddle = new Paddle(new ComputerController(this.ball), Side.Top);
            Paddle playerPaddle = new Paddle(new LocalPlayerController(), Side.Bottom);
            this.scoreDisplay = new ScoreDisplay(this.state.ScoreState);

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
                playerPaddle,
                computerPaddle
            };

            this.InitDrawables();
        }

        public override bool ButtonPressed(EventArgsInput e)
        {
            bool result = base.ButtonPressed(e);

            switch (e.Button)
            {
                case SButton.P:
                    this.TogglePaused();
                    return true;
                case SButton.Escape:
                    this.OnSwitchToNewMenu(new StartMenu());
                    return true;
            }

            return result;
        }

        public override void Update()
        {
            if (!this.state.Starting && !this.state.Paused)
            {
                bool collided = false;
                foreach (INonReactiveDrawableCollideable collideable in this.nonReactiveCollideables)
                {
                    if (this.ball.Bounds.Intersects(collideable.Bounds))
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

                if (collided && this.state.BallCollidedLastFrame) this.ball.Reset();

                this.state.BallCollidedLastFrame = collided;

                this.ball.Update();
                this.scoreDisplay.Update();
            }
            else if (this.state.Starting)
            {
                if (this.state.StartTimer % 60 == 0)
                    SoundManager.PlayCountdownSound();
                this.state.StartTimer--;
                if (this.state.StartTimer == 0) this.state.Starting = false;
            }
        }

        protected override IEnumerable<IDrawable> GetDrawables()
        {
            foreach (INonReactiveDrawableCollideable collideable in this.nonReactiveCollideables)
                yield return collideable;

            yield return this.ball;
            yield return this.scoreDisplay;

            yield return new ConditionalElement(
                new StaticTextElement("Press P to resume", ScreenWidth / 2, ScreenHeight / 2, true, true),
                () => this.state.Paused);

            yield return new ConditionalElement(
                new DynamicTextElement(() => $"{this.state.StartTimer / 60 + 1}", ScreenWidth / 2, ScreenHeight / 2, true, true),
                () => !this.state.Paused && this.state.Starting);

            yield return new ConditionalElement(new StaticTextElement("P to pause", 50, 150, true, true),
                () => !this.state.Paused && !this.state.Starting);

            yield return new ConditionalElement(new StaticTextElement("Esc to exit", 50, 100, true, true),
                () => !this.state.Starting);
        }


        private void Reset(bool resetScore)
        {
            if (this.state.Starting)
                return;
   
            int one = this.state.ScoreState.PlayerOneScore;
            int two = this.state.ScoreState.PlayerTwoScore;

            this.state.Reset();

            // This is bad
            if(!resetScore)
            {
                this.state.ScoreState.PlayerTwoScore = one;
                this.state.ScoreState.PlayerOneScore = two;
            }

            foreach (IResetable resetable in this.resetables)
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
            if (this.state.Starting)
                return;

            this.state.Starting = true;
        }

        private void TogglePaused()
        {
            if (this.state.Starting)
                return;

            SoundManager.PlayKeyPressSound();
            this.state.Paused = !this.state.Paused;
        }
    }
}