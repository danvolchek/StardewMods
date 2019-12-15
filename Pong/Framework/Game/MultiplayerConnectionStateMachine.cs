using Pong.Framework.Enums;
using StardewModdingAPI;

namespace Pong.Framework.Game
{
    internal class MultiplayerConnectionStateMachine
    {
        public MultiplayerConnectionState State { get; private set; }

        public MultiplayerConnectionStateMachine()
        {
            this.State = MultiplayerConnectionState.InPlayerLobby;
        }

        public void TransitionTo(MultiplayerConnectionState newState)
        {
            if (this.CouldTransitionTo(newState))
                this.State = newState;
            else
                ModEntry.Instance.Monitor.Log($"Tried to transition from {this.State} to {newState}, but that's illegal.", LogLevel.Error);
        }

        //Normal Flow (no errors):
        //     v------------------- Request Denied ----------------------<---------------------------<                                    >-- Play --v
        //     |                                                         |                           |                                    |          |
        //     V                                                         |                           |                                    |          |
        // InPlayerLobby ------- Send Challenge Request  ---> AwaitingChallengeRequestReponse -------+------- Request Accepted -------> InGame <-----<
        // ^  ^   |                                                                                  |                                    ^ |
        // |  |   |                                                                                  |                                    | |
        // |  |   >----------- Receive Challenge Request ---> ReceivedChallengeRequest --------------^                                    | |
        // |  |                                                   |      |                                                                | |
        // |  |                                                   |      |                                                                | |
        // |  ^-------------------- Send Deny Reply --------------<      >----- Send Accept Reply ----------------------------------------^ |
        // |                                                                                                                                |
        // ^-------------------------------------------------- Player Left Game ------------------------------------------------------------<
        //
        // Any error from the other player sends you back to InPlayerLobby, self disconnects return you to the server lobby.
        public bool CouldTransitionTo(MultiplayerConnectionState newState)
        {
            switch (this.State)
            {
                // We can always return to the lobby - either the requests timed out, a player left the game, a request was declined, etc.
                case MultiplayerConnectionState.InPlayerLobby:
                    break;
                // We can only move to awaiting a response after sending a request from the lobby.
                case MultiplayerConnectionState.AwaitingChallengeRequestReponse:
                    if (this.State != MultiplayerConnectionState.InPlayerLobby)
                        return false;
                    break;
                // We can only receive a request whilst in the lobby.
                case MultiplayerConnectionState.ReceivedChallengeRequest:
                    if (this.State != MultiplayerConnectionState.InPlayerLobby)
                        return false;
                    break;
                // We can only move to in game after waiting for a/ sending an accepted response.
                case MultiplayerConnectionState.InGame:
                    if (this.State != MultiplayerConnectionState.AwaitingChallengeRequestReponse)
                        return false;
                    break;
            }

            return true;
        }
    }
}
