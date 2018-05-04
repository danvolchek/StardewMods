using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ChatCommands
{
    /// <summary>
    ///     Replaces the game's chat box.
    /// </summary>
    internal class CommandChatBox : ChatBox
    {
        private readonly List<string> bCheatHistory;

        private readonly IReflectedField<int> bCheatHistoryPosition;
        private readonly IReflectedField<bool> bChoosingEmoji;
        private readonly ClickableTextureComponent bEmojiMenuIcon;
        private readonly IReflectedMethod bFormatMessage;
        private readonly List<ChatMessage> bMessages;
        private readonly ICommandHandler handler;

        private readonly int maxHistoryEntries;

        //Math.Min(messages.Count() - 1,10) when messages messages.Count() <= 10
        //Decrease to scroll up (show older messages) increase to scroll down (show newer messages)
        private int displayMessageIndex;

        private string savedText;
        //cant add text to first command in history (why?wtf)
        //scroll bar might be a little too low

        public CommandChatBox(IReflectionHelper helper, ICommandHandler handler, ChatCommandsConfig config)
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

            this.displayMessageIndex = -1;
            this.maxHistoryEntries = config.MaximumNumberOfHistoryMessages;
            this.DetermineNumberOfMaxMessages();
        }

        private void EnterPressed(TextBox sender)
        {
            if (sender is ChatTextBox chatTextBox)
            {
                if (chatTextBox.finalText.Count > 0)
                {
                    string message = ChatMessage.makeMessagePlaintext(chatTextBox.finalText);
                    if (message.Length < 1)
                    {
                        this.textBoxEnter(sender);
                        return;
                    }

                    string filtered = FilterMessagePlaintext(message);
                    if (message[0] == 47 && this.handler.CanHandle(filtered))
                    {
                        this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 0,
                            LocalizedContentManager.CurrentLanguageCode, message);
                        this.handler.Handle(filtered);
                        this.bCheatHistory.Insert(0, filtered);
                        if (this.bCheatHistory.Count >= this.maxHistoryEntries)
                            this.bCheatHistory.RemoveAt(this.bCheatHistory.Count - 1);
                    }
                    else
                    {
                        this.textBoxEnter(sender);
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
            this.displayMessageIndex = this.bMessages.Count - 1;
        }

        public override bool isWithinBounds(int x, int y)
        {
            if (x - this.xPositionOnScreen < this.width && x - this.xPositionOnScreen >= 0 &&
                y - this.yPositionOnScreen < this.height &&
                y - this.yPositionOnScreen >= -this.GetOldMessagesBoxHeight())
                return true;
            return this.bChoosingEmoji.GetValue() && this.emojiMenu.isWithinBounds(x, y);
        }

        private int GetOldMessagesBoxHeight()
        {
            return this.GetDisplayedLines().Select(item => item.verticalSize).Sum() + 20;
        }

        public override void receiveChatMessage(long sourceFarmer, int chatKind,
            LocalizedContentManager.LanguageCode language, string message)
        {
            string text1 = this.bFormatMessage.Invoke<string>(sourceFarmer, chatKind, message);
            string text2 = FixedParseText(text1, this.chatBox.Font, this.chatBox.Width - 16);

            foreach (string part in text2.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                this.AddNewMessage(part, this.messageColor(chatKind), language);
            }
        }

        public override void addMessage(string message, Color color)
        {
            string text = FixedParseText(message, this.chatBox.Font, this.chatBox.Width - 8);

            foreach (string part in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                this.AddNewMessage(part, color, LocalizedContentManager.CurrentLanguageCode);
            }
        }

        private void AddNewMessage(string message, Color color, LocalizedContentManager.LanguageCode code)
        {
            if (string.IsNullOrEmpty(message))
                message = " ";

            ChatMessage newMessage = new ChatMessage
            {
                timeLeftToDisplay = 600,
                verticalSize = (int)this.chatBox.Font.MeasureString(message).Y + 4,
                color = color,
                language = code
            };
            newMessage.parseMessageForEmoji(message);
            this.bMessages.Add(newMessage);
            if (this.maxHistoryEntries > 0 && this.bMessages.Count >= this.maxHistoryEntries)
                this.bMessages.RemoveAt(0);
            else if (this.displayMessageIndex == this.bMessages.Count - 2) this.displayMessageIndex++;
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
            string plaintext = FilterMessagePlaintext(ChatMessage.makeMessagePlaintext(this.chatBox.finalText));

            return plaintext.Length == 0;
        }

        /// <summary>Filters generated plaintext by removing color from the end of the name.</summary>
        private static string FilterMessagePlaintext(string input)
        {
            if (Game1.player.defaultChatColor == null ||
                ChatMessage.getColorFromName(Game1.player.defaultChatColor).Equals(Color.White)) return input;
            return input.EndsWith($" [{Game1.player.defaultChatColor}]")
                ? input.Substring(0,
                    input.LastIndexOf($" [{Game1.player.defaultChatColor}]",
                        StringComparison.InvariantCultureIgnoreCase))
                : input;
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);

            this.DetermineNumberOfMaxMessages();
        }

        private void DetermineNumberOfMaxMessages()
        {
            this.maxMessages = 10;

            float lineSize = messageFont(LocalizedContentManager.CurrentLanguageCode).MeasureString("(").Y + 4;
            float currPos = this.yPositionOnScreen - lineSize * this.maxMessages;
            while (currPos >= Game1.tileSize * 2)
            {
                this.maxMessages++;
                currPos -= lineSize;
            }

            this.maxMessages--;
        }

        protected override void runCommand(string command)
        {
            base.runCommand(command);
            if (this.bMessages.Count <= this.displayMessageIndex)
                this.displayMessageIndex = this.bMessages.Count - 1;
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Up)
            {
                if (this.bCheatHistoryPosition.GetValue() >= this.bCheatHistory.Count - 1)
                    return;

                if (this.bCheatHistoryPosition.GetValue() == -1)
                    this.savedText =
                        FilterMessagePlaintext(ChatMessage.makeMessagePlaintext(this.chatBox.finalText));

                this.bCheatHistoryPosition.SetValue(this.bCheatHistoryPosition.GetValue() + 1);
                this.SimulatePlayerReset(this.bCheatHistory[this.bCheatHistoryPosition.GetValue()]);
            }
            else if (key == Keys.Down)
            {
                if (this.bCheatHistoryPosition.GetValue() <= 0)
                {
                    if (this.bCheatHistoryPosition.GetValue() != 0) return;
                    //This makes the user able to type in the box again.
                    int currIndex = this.displayMessageIndex;
                    this.clickAway();
                    this.activate();
                    this.displayMessageIndex = currIndex;
                    this.SimulatePlayerReset(this.savedText);

                    return;
                }

                this.bCheatHistoryPosition.SetValue(this.bCheatHistoryPosition.GetValue() - 1);
                this.SimulatePlayerReset(this.bCheatHistory[this.bCheatHistoryPosition.GetValue()]);
            }
        }

        private bool ScrollDown(bool probe = false)
        {
            if (this.displayMessageIndex >= this.bMessages.Count - 1) return false;
            if (probe)
                return true;
            this.displayMessageIndex++;

            return true;
        }

        private bool ScrollUp(bool probe = false)
        {
            if (this.displayMessageIndex <= Math.Min(this.maxMessages, this.bMessages.Count) - 1) return false;
            if (probe)
                return true;
            this.displayMessageIndex--;

            return true;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (!this.bChoosingEmoji.GetValue())
            {
                //<0 = down
                if (direction < 0)
                {
                    this.ScrollDown();
                }
                else
                {
                    this.ScrollUp();
                }
            }
            else
            {
                this.emojiMenu.receiveScrollWheelAction(direction);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (this.GetDisplayedLines().Any())
            {
                int windowHeight = this.GetOldMessagesBoxHeight() - 20;

                drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), this.xPositionOnScreen,
                    this.yPositionOnScreen - windowHeight - 20 + (this.chatBox.Selected ? 0 : this.chatBox.Height),
                    this.chatBox.Width, windowHeight + 20, Color.White, 4f, false);

                if (this.bMessages.Count > this.maxMessages && this.isActive())
                {
                    int barWindowHeight = windowHeight - 51;

                    int numScrolls = this.bMessages.Count - this.maxMessages + 1;
                    int barHeight = barWindowHeight / numScrolls;
                    int barOffset = 7 - 20 + 24 + 3 + barWindowHeight -
                                    (this.bMessages.Count - this.displayMessageIndex) * barWindowHeight / numScrolls;
                                       
                    int topOffset = 7 - 20 + windowHeight -
                                    (this.bMessages.Count - this.maxMessages + 1) * (windowHeight) / numScrolls;

                    int bottomOffset = 7 - 20 + windowHeight -
                                    (this.bMessages.Count - (this.bMessages.Count)) * (windowHeight) / numScrolls;
                    //bottom arrow - by popular demand, always show arrows
                    //if (this.ScrollDown(true))
                    b.Draw(Game1.mouseCursors, new Vector2(this.xPositionOnScreen + this.width - 32 - 3,
                            this.yPositionOnScreen - windowHeight + (this.chatBox.Selected ? 0 : this.chatBox.Height) +
                            bottomOffset - 24),
                        new Rectangle(421, 472, 11, 12), Color.White, 0, Vector2.Zero, Vector2.One * 2,
                        SpriteEffects.None, 0);
                    //top arrow - by popular demand, always show arrows
                    //if (this.ScrollUp(true))
                    b.Draw(Game1.mouseCursors, new Vector2(this.xPositionOnScreen + this.width - 32 - 3,
                            this.yPositionOnScreen - windowHeight + (this.chatBox.Selected ? 0 : this.chatBox.Height) +
                            topOffset + 5),
                        new Rectangle(421, 459, 11, 12), Color.White, 0, Vector2.Zero, Vector2.One * 2,
                        SpriteEffects.None, 0);

                    //scrollbar
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(325, 448, 5, 17),
                        this.xPositionOnScreen + this.width - 32,
                        this.yPositionOnScreen - windowHeight + (this.chatBox.Selected ? 0 : this.chatBox.Height) +
                        barOffset, 16, barHeight, Color.White, 4f, false);
                }
            }

            int num2 = 0;
            foreach (ChatMessage message in this.GetDisplayedLines())
            {
                num2 += message.verticalSize;
                message.draw(b, 12,
                    this.yPositionOnScreen - num2 - 8 + (this.chatBox.Selected ? 0 : this.chatBox.Height));
            }

            if (!this.chatBox.Selected)
                return;
            this.chatBox.Draw(b, false);
            this.bEmojiMenuIcon.draw(b, Color.White, 0.99f);
            if (!this.bChoosingEmoji.GetValue())
                return;
            this.emojiMenu.draw(b);
        }

        private IEnumerable<ChatMessage> GetDisplayedLines()
        {
            for (int i = this.displayMessageIndex; i >= this.GetEndIndex(); i--)
            {
                ChatMessage message = this.bMessages[i];
                if (this.chatBox.Selected || message.alpha > 0.00999999977648258)
                    yield return message;
            }
        }

        private int GetEndIndex()
        {
            if (this.displayMessageIndex < this.maxMessages)
                return 0;

            return this.displayMessageIndex - this.maxMessages + 1;
        }

        private static string FixedParseText(string text, SpriteFont whichFont, int width)
        {
            if (text == null)
                return "";
            string str1 = string.Empty;
            string str2 = string.Empty;
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja ||
                LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh ||
                LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.th)
            {
                foreach (char ch in text)
                {
                    if (whichFont.MeasureString(str1 + ch).X > (double)width)
                    {
                        str2 = str2 + str1 + Environment.NewLine;
                        str1 = string.Empty;
                    }

                    str1 += ch.ToString();
                }

                return str2 + str1;
            }

            string str3 = text;
            char[] chArray = { ' ' };
            foreach (string str4 in str3.Split(chArray))
                try
                {
                    if (whichFont.MeasureString(str1 + str4).X > (double)width || str4.Equals(Environment.NewLine))
                    {
                        str2 = str2 + str1 + Environment.NewLine;
                        str1 = string.Empty;
                    }

                    if (!str4.Equals(Environment.NewLine))
                        str1 = str1 + str4 + " ";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception measuring string: " + ex);
                }

            return str2 + str1;
        }
    }
}