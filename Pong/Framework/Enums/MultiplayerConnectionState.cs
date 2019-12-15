namespace Pong.Framework.Enums
{
    internal enum MultiplayerConnectionState
    {
        InPlayerLobby,

        AwaitingChallengeRequestReponse,
        ReceivedChallengeRequest,

        InGame
    }
}
