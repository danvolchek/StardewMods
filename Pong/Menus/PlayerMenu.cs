using Pong.Framework.Common;
using Pong.Framework.Enums;
using Pong.Framework.Game;
using Pong.Framework.Menus;
using Pong.Framework.Menus.Elements;
using Pong.Framework.Messages;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System.Collections.Generic;
using System.Linq;

namespace Pong.Menus
{
    class PlayerMenu : Menu
    {
        private readonly IList<IMultiplayerPeer> peers;
        private readonly ElementContainer opponentList = new ElementContainer();

        private readonly MultiplayerConnectionStateMachine multiplayerConnectionState = new MultiplayerConnectionStateMachine();
        private long currentChallengeRequestRecipient;

        public PlayerMenu()
        {
            this.peers = ModEntry.Instance.Helper.Multiplayer.GetConnectedPlayers().Where(peer => peer.PlayerID != Game1.player.UniqueMultiplayerID).ToList();
            this.UpdatePeers();

            ModEntry.Instance.Helper.Events.Multiplayer.PeerContextReceived += this.Multiplayer_PeerContextReceived;
            ModEntry.Instance.Helper.Events.Multiplayer.PeerDisconnected += this.Multiplayer_PeerDisconnected;
            ModEntry.Instance.Helper.Events.Multiplayer.ModMessageReceived += this.Multiplayer_ModMessageReceived;


            this.InitDrawables();
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
                case SButton.Escape:
                    this.OnSwitchToNewMenu(new StartMenu());
                    return true;
            }

            return result;
        }

        private void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModEntry.Instance.PongId)
                return;

            if (e.Type == typeof(ChallengeRequestMessage).Name)
            {
                if (this.multiplayerConnectionState.State == MultiplayerConnectionState.AwaitingChallengeRequestReponse)
                {
                    //If the requesting player is the same as the player we sent a request to, accept. Send them back an accept and move to InGame.
                    //Otherwise, send them a deny and keep waiting.
                    if (this.currentChallengeRequestRecipient == e.FromPlayerID)
                    {
                        this.SendChallengeResponse(e.FromPlayerID, true);
                        this.MoveToGame(e.FromPlayerID);
                    }
                    else
                        this.SendChallengeResponse(e.FromPlayerID, false, "That player is waiting on another challenge request.");

                }
                else if (this.multiplayerConnectionState.State == MultiplayerConnectionState.InPlayerLobby)
                {
                    this.multiplayerConnectionState.TransitionTo(MultiplayerConnectionState.ReceivedChallengeRequest);
                    this.currentChallengeRequestRecipient = e.FromPlayerID; // rename to current challenge request sender?

                    //Show a modal to the player asking them if they want to accept.
                }
                else if (this.multiplayerConnectionState.State == MultiplayerConnectionState.InGame)
                    this.SendChallengeResponse(e.FromPlayerID, false, "That player is already in a game.");
                else if (this.multiplayerConnectionState.State == MultiplayerConnectionState.ReceivedChallengeRequest)
                    this.SendChallengeResponse(e.FromPlayerID, false, "That player already has a challenge request.");
            }
            else if (e.Type == typeof(ChallengeRequestResponseMessage).Name)
            {

                if (this.multiplayerConnectionState.State == MultiplayerConnectionState.AwaitingChallengeRequestReponse)
                {
                    bool accepted = e.ReadAs<ChallengeRequestResponseMessage>().Accepted;

                    if (!accepted)
                        this.multiplayerConnectionState.TransitionTo(MultiplayerConnectionState.InPlayerLobby);
                    else
                        this.MoveToGame(e.FromPlayerID);
                }
                //Otherwise, ignore the message. If we're in game or not awaiting a response, ignore the message.
            }
        }

        private void UpdatePeers()
        {
            int peerHeight = StaticTextElement.HighlightWidth + SpriteText.getHeightOfString("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            int peerStart = ScreenHeight / 3;

            this.opponentList.Elements.Clear();
            for (int index = 0; index < this.peers.Count; index++)
            {
                IMultiplayerPeer peer = this.peers[index];
                // This is a dynamic element b/c when we add it here the farmer doesn't exist yet
                this.opponentList.Elements.Add(new DynamicTextElement(() => Game1.getFarmer(peer.PlayerID).Name,
                    ScreenWidth / 2, peerStart + peerHeight * index, true, false, () => this.Challenge(peer.PlayerID)));
            }
        }

        private void Challenge(long player)
        {
            if (this.multiplayerConnectionState.CouldTransitionTo(MultiplayerConnectionState.AwaitingChallengeRequestReponse))
            {
                ModEntry.Instance.Monitor.Log("SENT CHALLENGE", LogLevel.Error);
                this.currentChallengeRequestRecipient = player;
                MailBox.Send(new MessageEnvelope(new ChallengeRequestMessage(), player));
                this.multiplayerConnectionState.TransitionTo(MultiplayerConnectionState.AwaitingChallengeRequestReponse);
            }
            else if (this.multiplayerConnectionState.State == MultiplayerConnectionState.ReceivedChallengeRequest &&
                     player == this.currentChallengeRequestRecipient)
            {
                this.SendChallengeResponse(player, true);
                this.MoveToGame(player);
            }
            //TODO: remove else if block after modals
        }

        private void MoveToGame(long enemyPlayer)
        {
            this.OnSwitchToNewMenu(new GameMenu(enemyPlayer));
        }

        private void SendChallengeResponse(long player, bool accepted, string reason = null)
        {
            MailBox.Send(new MessageEnvelope(new ChallengeRequestResponseMessage(accepted, reason), player));
        }

        private void Multiplayer_PeerDisconnected(object sender, PeerDisconnectedEventArgs e)
        {
            if (this.peers.Remove(e.Peer))
                this.UpdatePeers();
        }

        private void Multiplayer_PeerContextReceived(object sender, PeerContextReceivedEventArgs e)
        {
            if (e.Peer.HasSmapi && e.Peer.GetMod(ModEntry.Instance.PongId) != null)
            {
                this.peers.Add(e.Peer);
                this.UpdatePeers();
            }
        }

        protected override IEnumerable<IDrawable> GetDrawables()
        {
            yield return new StaticTextElement("Player Menu!", ScreenWidth / 2, 50, true, true);

            int escHeight = SpriteText.getHeightOfString("Press Esc to go back");
            yield return new StaticTextElement("Press Esc to go back", 15, ScreenHeight - escHeight - 15, false, false, () => this.OnSwitchToNewMenu(new StartMenu()));

            yield return this.opponentList;
        }

        public override void BeforeMenuSwitch()
        {
            ModEntry.Instance.Helper.Events.Multiplayer.PeerContextReceived -= this.Multiplayer_PeerContextReceived;
            ModEntry.Instance.Helper.Events.Multiplayer.PeerDisconnected -= this.Multiplayer_PeerDisconnected;
            ModEntry.Instance.Helper.Events.Multiplayer.ModMessageReceived -= this.Multiplayer_ModMessageReceived;
        }

        public override void Update()
        {

        }

        public override void Resize()
        {

        }
    }
}
