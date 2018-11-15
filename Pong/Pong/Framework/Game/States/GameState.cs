using System;

namespace Pong.Framework.Game.States
{
    internal class GameState: IState<GameState>
    {
        public bool BallCollidedLastFrame { get; set; }
        public bool Starting { get; set; } = true;
        public int StartTimer { get; set; } = 180;
        public bool Paused { get; set; }

        public VelocityState VelocityState { get; set; } = new VelocityState();
        public ScoreState ScoreState { get; set; } = new ScoreState();
        public PositionState PositionState { get; set; } = new PositionState();

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

            this.VelocityState.SetState(state.VelocityState);
            this.ScoreState.SetState(state.ScoreState);
            this.PositionState.SetState(state.PositionState);
        }

        public void Reset()
        {
            this.BallCollidedLastFrame = false;
            this.Starting = true;
            this.StartTimer = 180;
            this.Paused = false;

            this.ScoreState.Reset();
            this.VelocityState.Reset();
            this.PositionState.Reset();
        }
    }
}
