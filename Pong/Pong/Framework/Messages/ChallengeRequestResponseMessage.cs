namespace Pong.Framework.Messages
{
    class ChallengeRequestResponseMessage
    {
        public bool Accepted { get; }
        public string Reason { get; }

        public ChallengeRequestResponseMessage(bool accepted, string reason = null)
        {
            this.Accepted = accepted;
            this.Reason = reason;
        }
    }
}
