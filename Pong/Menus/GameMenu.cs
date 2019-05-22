using System;
using System.Collections.Generic;
using System.Threading;
using Pong.Framework.Common;
using Pong.Framework.Enums;
using Pong.Framework.Game;
using Pong.Framework.Game.States;
using Pong.Framework.Menus;
using Pong.Framework.Menus.Elements;
using Pong.Framework.Messages;
using Pong.Game;
using Pong.Game.Controllers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using IDrawable = Pong.Framework.Common.IDrawable;

namespace Pong.Menus
{
    internal class GameMenu : Menu, IResetable
    {
        private readonly List<INonReactiveDrawableCollideable> nonReactiveCollideables;
        private readonly List<IResetable> resetables;
        private readonly ScoreDisplay scoreDisplay;
        private readonly LocalPlayerController localPlayerController;

        private readonly Ball ball;
        private readonly GameState state = new GameState();
        private readonly long enemyPlayerId;

        private Thread syncDataThread;
        private bool shouldSyncData = true;
        private readonly bool isMultiplayerGame;
        private readonly bool isLeader;

        private readonly PositionState followerPaddlePosition = new PositionState();

        private bool paused;

        private const int MessageSendFrequency = 400;

        private double lastSentMessageTimestamp;

        public GameMenu(long? enemyPlayer = null)
        {

            this.isMultiplayerGame = enemyPlayer.HasValue;
            if (this.isMultiplayerGame)
                this.enemyPlayerId = enemyPlayer.Value;
            this.isLeader = this.isMultiplayerGame && Game1.player.UniqueMultiplayerID < this.enemyPlayerId;

            this.ball = new Ball(this.state.BallPositionState, this.state.BallVelocityState);
            this.scoreDisplay = new ScoreDisplay(this.state.ScoreState);

            Paddle playerOnePaddle;
            Paddle playerTwoPaddle;

            if (!this.isMultiplayerGame)
            {
                playerTwoPaddle = new Paddle(new ComputerController(this.ball), Side.Top);
                playerOnePaddle = new Paddle(this.localPlayerController = new LocalPlayerController(this.state.PaddlePositionState), Side.Bottom);
            }
            else if (this.isLeader)
            {
                playerTwoPaddle = new Paddle(new StatePaddleController(this.followerPaddlePosition), Side.Top);
                playerOnePaddle = new Paddle(this.localPlayerController = new LocalPlayerController(this.state.PaddlePositionState), Side.Bottom);
            }
            else
            {
                playerTwoPaddle = new Paddle(new StatePaddleController(this.state.PaddlePositionState), Side.Top);
                playerOnePaddle = new Paddle(this.localPlayerController = new LocalPlayerController(this.followerPaddlePosition), Side.Bottom);
            }

            this.nonReactiveCollideables = new List<INonReactiveDrawableCollideable>
            {
                playerOnePaddle,
                playerTwoPaddle,
                new Wall(Side.Bottom),
                new Wall(Side.Left),
                new Wall(Side.Right),
                new Wall(Side.Top)
            };

            this.resetables = new List<IResetable>
            {
                playerOnePaddle
            };

            this.InitDrawables();

            if (this.isMultiplayerGame)
            {
                this.InitSync();
            }
            else
                this.resetables.Add(playerTwoPaddle);
        }

        private void InitSync()
        {
            if (this.isLeader)
                this.syncDataThread = new Thread(this.SyncGameState);
            else
            {
                this.state.BallVelocityState.Invert();
                this.syncDataThread = new Thread(this.SendMyPaddlePosition);
            }

            ModEntry.Instance.Helper.Events.Multiplayer.ModMessageReceived += this.Multiplayer_ModMessageReceived;

            this.syncDataThread.Start();
        }

        private void SyncGameState()
        {
            while (this.shouldSyncData)
            {
                this.state.CurrentTime = GetTimestamp();

                if (this.state.CurrentTime - this.lastSentMessageTimestamp > MessageSendFrequency)
                {
                    this.state.All = false;
                    MailBox.Send(new MessageEnvelope(this.state, this.enemyPlayerId));
                    this.state.All = true;
                    this.lastSentMessageTimestamp = this.state.CurrentTime;
                }
            }
        }

        private void ForceSyncGameState()
        {
            this.state.CurrentTime = GetTimestamp();
            MailBox.Send(new MessageEnvelope(this.state, this.enemyPlayerId));
            this.lastSentMessageTimestamp = this.state.CurrentTime;
        }

        private void SendMyPaddlePosition()
        {
            while (this.shouldSyncData)
            {
                MailBox.Send(new MessageEnvelope(this.followerPaddlePosition, this.enemyPlayerId));
                Thread.Sleep(MessageSendFrequency);
            }
        }

        private void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModEntry.Instance.PongId)
                return;
            if (e.Type == typeof(GameState).Name && !this.isLeader)
            {
                GameState newState = e.ReadAs<GameState>();
                double diff = GetTimestamp() - newState.CurrentTime;

                newState.BallVelocityState.Invert();
                newState.BallPositionState.Invert();
                newState.PaddlePositionState.Invert();

                this.state.SetState(newState);

                //diff milli seconds, 60 frames per second -> need to run diff * 60

                //for (int i = 0; i < (diff * 60) / 1000; i++)
                //    this.Update();

                //if ((int)((diff * 60) / 1000) > 0)
                //    ModEntry.Instance.Monitor.Log($"Follower updated {(int)((diff * 60) / 1000)} times");

            }
            else if (e.Type == typeof(PositionState).Name && this.isLeader)
            {
                PositionState newState = e.ReadAs<PositionState>();
                newState.Invert();
                this.followerPaddlePosition.SetState(newState);
            }

        }

        private static double GetTimestamp()
        {
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public override void BeforeMenuSwitch()
        {
            if (this.enemyPlayerId != default(long))
                ModEntry.Instance.Helper.Events.Multiplayer.ModMessageReceived -= this.Multiplayer_ModMessageReceived;
            if (this.syncDataThread != null)
                this.shouldSyncData = false;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event arguments.</param>
        public override bool OnButtonPressed(ButtonPressedEventArgs e)
        {
            bool result = base.OnButtonPressed(e);

            if (this.CurrentModal != null)
                return result;

            switch (e.Button)
            {
                case SButton.P:
                    return this.TogglePaused();
                case SButton.Escape:
                    this.OnSwitchToNewMenu(new StartMenu());
                    return true;
            }

            return result;
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event arguments.</param>
        public override void OnCursorMoved(CursorMovedEventArgs e)
        {
            base.OnCursorMoved(e);
            this.localPlayerController.OnCursorMoved(e);
        }

        public override void Update()
        {
            if (!this.state.Starting && !this.paused)
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
                                this.ForceSyncGameState();
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

                if(this.state.BallCollidedLastFrame)
                    this.ForceSyncGameState();
            }
            else if (this.state.Starting)
            {
                if (this.state.StartTimer % 60 == 0)
                    SoundManager.PlayCountdownSound();
                this.state.StartTimer--;
                if (this.state.StartTimer == 0) this.state.Starting = false;

                this.ForceSyncGameState();
            }
        }

        protected override IEnumerable<IDrawable> GetDrawables()
        {
            foreach (INonReactiveDrawableCollideable collideable in this.nonReactiveCollideables)
                yield return collideable;

            yield return this.ball;
            yield return this.scoreDisplay;

            if (!this.isMultiplayerGame)
                yield return new ConditionalElement(
                    new StaticTextElement("Press P to resume", ScreenWidth / 2, ScreenHeight / 2, true, true),
                    () => this.paused);

            yield return new ConditionalElement(
                new DynamicTextElement(() => $"{this.state.StartTimer / 60 + 1}", ScreenWidth / 2, ScreenHeight / 2, true, true),
                () => !this.paused && this.state.Starting);

            if (!this.isMultiplayerGame)
                yield return new ConditionalElement(new StaticTextElement("P to pause", 50, 150, true, true),
                () => !this.paused && !this.state.Starting);

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
            this.followerPaddlePosition.Reset();
            this.paused = false;

            // This is bad
            if (!resetScore)
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

        private bool TogglePaused()
        {
            if (this.state.Starting || this.isMultiplayerGame)
                return false;

            SoundManager.PlayKeyPressSound();
            this.paused = !this.paused;

            return true;
        }
    }
}
