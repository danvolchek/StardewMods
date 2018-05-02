using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace ChatCommands
{
    /// <summary>
    /// Replaces the game's chat box.
    /// </summary>
    internal class CommandChatBox : ChatBox
    {
        private ICommandHandler handler;

        private IReflectedField<int> bCheatHistoryPosition;
        private IReflectedField<bool> bChoosingEmoji;
        private IReflectedMethod bFormatMessage;
        private List<ChatMessage> bMessages;
        private List<string> bCheatHistory;
        private ClickableTextureComponent bEmojiMenuIcon;

        //Math.Min(messages.Count() - 1,10) when messages messages.Count() <= 10
        //Decrease to scroll up (show older messages) increase to scroll down (show newer messages)
        private int displayStartIndex;

        private int maxHistoryEntries;

        private string savedText;
        //cant add text to first command in history (why?wtf)
        //scroll bar might be a little too low

        public CommandChatBox(IReflectionHelper helper, ICommandHandler handler, ChatCommandsConfig config) : base()
        {
            this.handler = handler;
            this.bCheatHistoryPosition = helper.GetField<int>(this, "cheatHistoryPosition");
            this.bFormatMessage = helper.GetMethod(this, "formatMessage");
            this.bMessages = helper.GetField<List<ChatMessage>>(this, "messages").GetValue();
            this.bCheatHistory = helper.GetField<List<string>>(this, "cheatHistory").GetValue();
            this.bEmojiMenuIcon = helper.GetField<ClickableTextureComponent>(this, "emojiMenuIcon").GetValue();
            this.bChoosingEmoji = helper.GetField<bool>(this, "choosingEmoji");

            this.chatBox.OnEnterPressed -= helper.GetField<TextBoxEvent>(this, "e").GetValue();
            this.chatBox.OnEnterPressed += this.EnterPressed;

            this.displayStartIndex = -1;
            this.maxHistoryEntries = config.MaximumNumberOfHistoryMessages;
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
                        string filtered = this.FilterMessagePlaintext(message);
                        this.handler.Handle(filtered);
                        this.bCheatHistory.Insert(0, filtered);
                        if (this.bCheatHistory.Count >= this.maxHistoryEntries)
                            this.bCheatHistory.RemoveAt(this.bCheatHistory.Count - 1);
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

        public override void clickAway()
        {
            base.clickAway();
            this.displayStartIndex = this.bMessages.Count - 1;
        }

        public override bool isWithinBounds(int x, int y)
        {
            if (x - this.xPositionOnScreen < this.width && x - this.xPositionOnScreen >= 0 && (y - this.yPositionOnScreen < this.height && y - this.yPositionOnScreen >= -this.GetOldMessagesBoxHeight()))
                return true;
            if (this.bChoosingEmoji.GetValue())
                return this.emojiMenu.isWithinBounds(x, y);
            return false;
        }

        public int GetOldMessagesBoxHeight()
        {
            int num = 20;
            for (int index = this.displayStartIndex; index >= this.GetEndIndex(); --index)
            {
                ChatMessage message = this.bMessages[index];
                if (this.chatBox.Selected || (double)message.alpha > 0.00999999977648258)
                    num += message.verticalSize;
            }
            return num;
        }

        public override void receiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
        {
            string text1 = this.bFormatMessage.Invoke<string>(sourceFarmer, chatKind, message);
            ChatMessage chatMessage = new ChatMessage();
            SpriteFont font = this.chatBox.Font;
            int width = this.chatBox.Width - 16;
            string text2 = Game1.parseText(text1, font, width);
            chatMessage.timeLeftToDisplay = 600;
            chatMessage.verticalSize = (int)this.chatBox.Font.MeasureString(text2).Y + 4;
            chatMessage.color = this.messageColor(chatKind);
            chatMessage.language = language;
            chatMessage.parseMessageForEmoji(text2);
            this.bMessages.Add(chatMessage);
            if (this.maxHistoryEntries > 0 && this.bMessages.Count >= this.maxHistoryEntries)
                this.bMessages.RemoveAt(0);
            else if (this.displayStartIndex == this.bMessages.Count - 2)
                this.displayStartIndex++;
        }

        public override void addMessage(string message, Color color)
        {
            ChatMessage chatMessage = new ChatMessage();
            string text = Game1.parseText(message, this.chatBox.Font, this.chatBox.Width - 8);
            chatMessage.timeLeftToDisplay = 600;
            chatMessage.verticalSize = (int)this.chatBox.Font.MeasureString(text).Y + 4;
            chatMessage.color = color;
            chatMessage.language = LocalizedContentManager.CurrentLanguageCode;
            chatMessage.parseMessageForEmoji(text);
            this.bMessages.Add(chatMessage);
            if (this.maxHistoryEntries > 0 && this.bMessages.Count >= this.maxHistoryEntries)
                this.bMessages.RemoveAt(0);
            else if (this.displayStartIndex == this.bMessages.Count - 2)
                this.displayStartIndex++;
        }

        /// <summary>Sets the contexts of the text box to the input string.</summary>
        /// <remarks>Doing it this way means the player can edit the input.</remarks>
        private void SimulatePlayerReset(string input)
        {
            this.chatBox.backspace();
            while (!this.IsChatBoxEmpty())
                this.chatBox.backspace();
            foreach (char c in input.Trim())
                this.chatBox.RecieveTextInput(c);
        }

        private bool IsChatBoxEmpty()
        {
            string plaintext = this.FilterMessagePlaintext(ChatMessage.makeMessagePlaintext(this.chatBox.finalText));

            return plaintext.Length == 0;
        }

        /// <summary>Filters generated plaintext by removing color from the end of the name.</summary>
        private string FilterMessagePlaintext(string input)
        {
            if (Game1.player.defaultChatColor != null && !ChatMessage.getColorFromName(Game1.player.defaultChatColor).Equals(Color.White))
            {
                if (input.EndsWith($" [{Game1.player.defaultChatColor}]"))
                    return input.Substring(0, input.LastIndexOf($" [{Game1.player.defaultChatColor}]"));
            }

            return input;
        }

        protected override void runCommand(string command)
        {
            base.runCommand(command);
            if (this.bMessages.Count <= this.displayStartIndex)
                this.displayStartIndex = this.bMessages.Count - 1;
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Up)
            {
                if (this.bCheatHistoryPosition.GetValue() >= this.bCheatHistory.Count - 1)
                    return;

                if (this.bCheatHistoryPosition.GetValue() == -1)
                    this.savedText = this.FilterMessagePlaintext(ChatMessage.makeMessagePlaintext(this.chatBox.finalText));

                this.bCheatHistoryPosition.SetValue(this.bCheatHistoryPosition.GetValue() + 1);
                this.SimulatePlayerReset(this.bCheatHistory[this.bCheatHistoryPosition.GetValue()]);
            }
            else if (key == Keys.Down)
            {
                if (this.bCheatHistoryPosition.GetValue() <= 0)
                {
                    if (this.bCheatHistoryPosition.GetValue() == 0)
                    {
                        //This makes the user able to type in the box again.
                        int currIndex = this.displayStartIndex;
                        this.clickAway();
                        this.activate();
                        this.displayStartIndex = currIndex;
                        this.SimulatePlayerReset(this.savedText);
                    }
                    return;
                }
                this.bCheatHistoryPosition.SetValue(this.bCheatHistoryPosition.GetValue() - 1);
                this.SimulatePlayerReset(this.bCheatHistory[this.bCheatHistoryPosition.GetValue()]);
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (!this.bChoosingEmoji.GetValue())
            {
                //<0 = down
                if (direction < 0)
                {
                    if (this.displayStartIndex < this.bMessages.Count - 1)
                        this.displayStartIndex++;
                }
                else
                {
                    if (this.displayStartIndex > Math.Min(this.maxMessages, this.bMessages.Count) - 1)
                        this.displayStartIndex--;
                }
            }
            else
                this.emojiMenu.receiveScrollWheelAction(direction);
        }

        public override void draw(SpriteBatch b)
        {
            int num1 = 0;
            bool flag = false;
            for (int index = this.displayStartIndex; index >= this.GetEndIndex(); --index)
            {
                ChatMessage message = this.bMessages[index];
                if (this.chatBox.Selected || (double)message.alpha > 0.00999999977648258)
                {
                    num1 += message.verticalSize;
                    flag = true;
                }
            }
            if (flag)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), this.xPositionOnScreen, this.yPositionOnScreen - num1 - 20 + (this.chatBox.Selected ? 0 : this.chatBox.Height), this.chatBox.Width, num1 + 20, Color.White, 4f, false);
                if (this.bMessages.Count > this.maxMessages && this.isActive())
                {
                    int numScrolls = this.bMessages.Count - this.maxMessages + 1;
                    int barHeight = num1 / numScrolls;
                    int barOffset = 7 + num1 - (((this.bMessages.Count - this.displayStartIndex) * num1) / numScrolls);

                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(325, 448, 5, 17), this.xPositionOnScreen + this.width - 32, this.yPositionOnScreen - num1 - 20 + (this.chatBox.Selected ? 0 : this.chatBox.Height) + barOffset, 16, barHeight, Color.White, 4f, false);

                }
            }

            int num2 = 0;
            for (int index = this.displayStartIndex; index >= this.GetEndIndex(); --index)
            {
                ChatMessage message = this.bMessages[index];
                num2 += message.verticalSize;
                message.draw(b, 12, this.yPositionOnScreen - num2 - 8 + (this.chatBox.Selected ? 0 : this.chatBox.Height));
            }
            if (!this.chatBox.Selected)
                return;
            this.chatBox.Draw(b, false);
            this.bEmojiMenuIcon.draw(b, Color.White, 0.99f);
            if (!this.bChoosingEmoji.GetValue())
                return;
            this.emojiMenu.draw(b);
        }

        private int GetEndIndex()
        {
            if (this.displayStartIndex < this.maxMessages)
                return 0;

            return (this.displayStartIndex - this.maxMessages) + 1;
        }
    }
}