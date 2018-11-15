using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
