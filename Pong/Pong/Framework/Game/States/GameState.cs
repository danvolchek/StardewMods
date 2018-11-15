using System;

namespace Pong.Framework.Game.States
{
    internal class GameState: IState<GameState>
    {
        public bool BallCollidedLastFrame { get; set; }
        public bool Starting { get; set; } = true;
        public int StartTimer { get; set; } = 180;
        public bool Paused { get; set; }

        public ScoreState ScoreState { get; set; } = new ScoreState();
        public VelocityState BallVelocityState { get; set; } = new VelocityState();
        public PositionState BallPositionState { get; set; } = new PositionState();
        public PositionState OtherPaddlePositionState { get; set; } = new PositionState();

        public GameState()
        {
            this.Reset();
        }

        public void SetState(GameState state)
        {
            this.BallCollidedLastFrame = state.BallCollidedLastFrame;
            this.Starting = state.Starting;
            this.StartTimer = state.StartTimer;
            this.Paused = state.Paused;

            this.BallVelocityState.SetState(state.BallVelocityState);
            this.ScoreState.SetState(state.ScoreState);
            this.BallPositionState.SetState(state.BallPositionState);
        }

        public void Reset()
        {
            this.BallCollidedLastFrame = false;
            this.Starting = true;
            this.StartTimer = 180;
            this.Paused = false;

            this.ScoreState.Reset();
            this.BallVelocityState.Reset();
            this.BallPositionState.Reset();
        }
    }
}
