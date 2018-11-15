namespace Pong.Framework.Game.States
{
    internal class ScoreState: IState<ScoreState>
    {
        public int PlayerOneScore { get; set; }
        public int PlayerTwoScore { get; set; }

        public ScoreState()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.PlayerOneScore = this.PlayerTwoScore = 0;
        }

        public void SetState(ScoreState state)
        {
            this.PlayerTwoScore = state.PlayerTwoScore;
            this.PlayerTwoScore = state.PlayerTwoScore;
        }
    }
}
