using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ChatCommands
{
    /// <summary>
    /// Replaces the game's chat box.
    /// </summary>
    internal class CommandChatBox : ChatBox
    {
        private ICommandHandler handler;
        private IReflectedField<int> bCheatHistoryPosition;
        private IReflectedField<Multiplayer> multiplayer;

        public CommandChatBox(IReflectionHelper helper, ICommandHandler handler) : base()
        {
            this.handler = handler;
            this.bCheatHistoryPosition = helper.GetField<int>(this, "cheatHistoryPosition");
            this.multiplayer = helper.GetField<Multiplayer>(typeof(Game1), "multiplayer");

            this.chatBox.OnEnterPressed -= helper.GetField<TextBoxEvent>(this, "e").GetValue();
            this.chatBox.OnEnterPressed += this.EnterPressed;
        }

        public void EnterPressed(TextBox sender)
        {
            if (sender is ChatTextBox)
            {
                ChatTextBox chatTextBox = sender as ChatTextBox;
                if (chatTextBox.finalText.Count > 0)
                {
                    string message = ChatMessage.makeMessagePlaintext(chatTextBox.finalText);
                    if (message.Length < 1)
                    {
                        base.textBoxEnter(sender);
                        return;
                    }

                    if (message[0] == 47 && this.handler.CanHandle(message))
                    {
                        this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 0, LocalizedContentManager.CurrentLanguageCode, message);
                        this.handler.Handle(message);
                    }
                    else
                    {
                        base.textBoxEnter(sender);
                        return;
                    }
                }
                chatTextBox.reset();
                this.bCheatHistoryPosition.SetValue(-1);
            }
            sender.Text = "";
            this.clickAway();
        }
    }
}