using ChatCommands.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChatCommands.ClassReplacements
{
    /// <summary>Replaces the game's chat box.</summary>
    internal class CommandChatBox : ChatBox
    {
        internal const char WhisperSeparator = (char)250;

        private readonly IReflectedField<int> bCheatHistoryPosition;
        private readonly IReflectedField<bool> bChoosingEmoji;
        private readonly ClickableTextureComponent bEmojiMenuIcon;
        private readonly IReflectedMethod bFormatMessage;
        private readonly List<ChatMessage> bMessages;
        private readonly CommandChatTextBox commandChatTextBox;
        private readonly ChatCommandsConfig config;
        private readonly ICommandHandler handler;
        private readonly InputState inputState;

        private readonly Multiplayer multiplayer;
        private readonly List<CommandChatTextBoxState> sentMessageHistory = new List<CommandChatTextBoxState>();
        private CommandChatTextBoxState currentTypedMessage;
        private int displayLineIndex;

        private bool ignoreClickAway;
        private bool isEscapeDown;

        private bool sawChangeToTrue;

        /// <summary>Construct an instance.</summary>
        /// <remarks>Reassigns the enter handler, replaces <see cref="ChatTextBox" /> and <see cref="EmojiMenu" />.</remarks>
        public CommandChatBox(IModHelper helper, ICommandHandler handler, ChatCommandsConfig config)
        {
            this.handler = handler;
            this.bCheatHistoryPosition = helper.Reflection.GetField<int>(this, "cheatHistoryPosition");
            this.bFormatMessage = helper.Reflection.GetMethod(this, "formatMessage");
            this.bMessages = helper.Reflection.GetField<List<ChatMessage>>(this, "messages").GetValue();
            this.bEmojiMenuIcon =
                helper.Reflection.GetField<ClickableTextureComponent>(this, "emojiMenuIcon").GetValue();
            this.bChoosingEmoji = helper.Reflection.GetField<bool>(this, "choosingEmoji");
            Texture2D chatBoxTexture = Game1.content.Load<Texture2D>("LooseSprites\\chatBox");

            this.chatBox.OnEnterPressed -= helper.Reflection.GetField<TextBoxEvent>(this, "e").GetValue();
            this.chatBox = this.commandChatTextBox = new CommandChatTextBox(chatBoxTexture,
                null, Game1.smallFont, Color.White);
            Game1.keyboardDispatcher.Subscriber = this.chatBox;
            this.chatBox.Selected = false;
            this.chatBox.OnEnterPressed += this.EnterPressed;

            this.multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            this.inputState = helper.Reflection.GetField<InputState>(typeof(Game1), "input").GetValue();

            ConsoleChatMessage.Init(LocalizedContentManager.CurrentLanguageCode);

            this.emojiMenu = new CommandEmojiMenu(helper.Reflection, this, emojiTexture, chatBoxTexture);

            helper.Reflection.GetMethod(this, "updatePosition").Invoke();

            this.displayLineIndex = -1;
            this.config = config;
            this.config.UseMonospacedFontForCommandOutput = this.config.UseMonospacedFontForCommandOutput && !(
                                                                LocalizedContentManager.CurrentLanguageCode ==
                                                                LocalizedContentManager.LanguageCode.ja ||
                                                                LocalizedContentManager.CurrentLanguageCode ==
                                                                LocalizedContentManager.LanguageCode.zh ||
                                                                LocalizedContentManager.CurrentLanguageCode ==
                                                                LocalizedContentManager.LanguageCode.th);
            this.DetermineNumberOfMaxMessages();
        }

        /// <summary>Handle enter being pressed.</summary>
        public void EnterPressed(TextBox sender)
        {
            if (sender is ChatTextBox chatTextBox)
            {
                if (chatTextBox.finalText.Count > 0)
                {
                    string message = ChatMessage.makeMessagePlaintext(chatTextBox.finalText, Utils.ShouldIncludeColorInfo(chatTextBox.finalText));
                    if (message.Length < 1)
                    {
                        this.textBoxEnter(sender);
                        this.commandChatTextBox.Reset();
                        this.bCheatHistoryPosition.SetValue(-1);
                        return;
                    }

                    this.sentMessageHistory.Insert(0, this.commandChatTextBox.Save());
                    if (this.sentMessageHistory.Count >= this.config.MaximumNumberOfHistoryMessages)
                        this.sentMessageHistory.RemoveAt(this.sentMessageHistory.Count - 1);

                    string filtered = FilterMessagePlaintext(message);

                    Match whisperMatch;
                    if (message[0] == 47 && this.handler.CanHandle(filtered))
                    {
                        this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 0,
                            LocalizedContentManager.CurrentLanguageCode, message);
                        this.handler.Handle(filtered);
                    }
                    else if (this.commandChatTextBox.CurrentRecipientId != -1)
                    {
                        long key = Game1.player.UniqueMultiplayerID ^ this.commandChatTextBox.CurrentRecipientId;
                        string identifier = $"{this.commandChatTextBox.CurrentRecipientId}";

                        string playerName = Game1.getOnlineFarmers().FirstOrDefault(farmer =>
                            farmer.UniqueMultiplayerID == this.commandChatTextBox.CurrentRecipientId)?.Name;

                        if (playerName == null)
                        {
                            this.addMessage($"{this.commandChatTextBox.CurrentRecipientName} is offline now.",
                                Color.Red);
                        }
                        else
                        {
                            this.receiveChatMessage(Game1.player.UniqueMultiplayerID, 0,
                                LocalizedContentManager.CurrentLanguageCode,
                                $"{(char)(WhisperSeparator + 1)}{playerName}{WhisperSeparator}{message}");
                            this.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode,
                                $"{WhisperSeparator}{Utils.EncipherText(identifier, key)}{WhisperSeparator}{Utils.EncipherText(message, key)}", this.commandChatTextBox.CurrentRecipientId);
                        }
                    }
                    else if ((whisperMatch = CommandChatTextBox.WhisperRegex.Match(filtered)).Success)
                    {
                        string response = null;
                        if (!Context.IsMultiplayer)
                            response = "You can't send whispers in singleplayer.";

                        if (response == null)
                            response = whisperMatch.Groups[1].Value == Game1.player.Name
                                ? "You can't whisper to yourself."
                                : $"There isn't anyone named {whisperMatch.Groups[1].Value} online.";
                        this.addMessage(response, Color.Red);
                    }
                    else if (CommandChatTextBox.WhisperReplyRegex.Match(filtered).Success)
                    {
                        this.addMessage(!Context.IsMultiplayer ? "You can't reply to whispers in singleplayer." : "You can't reply when you haven't received any whispers.", Color.Red);
                    }
                    else
                    {
                        this.textBoxEnter(sender);
                        this.commandChatTextBox.Reset();
                        this.bCheatHistoryPosition.SetValue(-1);
                        return;
                    }
                }

                this.commandChatTextBox.Reset();
                this.bCheatHistoryPosition.SetValue(-1);
            }

            sender.Text = "";
            this.clickAway();
        }

        //breaks left clicks
        /// <summary>Handles deactiving the chat box.</summary>
        public override void clickAway()
        {
            KeyboardState keyboardState = this.inputState.GetKeyboardState();

            if (keyboardState.IsKeyDown(Keys.Escape) && (this.isEscapeDown || !this.sawChangeToTrue))
            {
                if (this.ignoreClickAway || !this.sawChangeToTrue)
                    return;

                this.ignoreClickAway = true;
            }

            this.DeactivateLayer();
        }

        public void EscapeStatusChanged(bool isDown)
        {
            if (!this.isEscapeDown && isDown)
                this.sawChangeToTrue = true;
            else
                this.sawChangeToTrue = false;

            this.isEscapeDown = isDown;
            if (!isDown)
                this.ignoreClickAway = false;
        }

        private void DeactivateLayer()
        {
            if (this.bChoosingEmoji.GetValue())
                this.bChoosingEmoji.SetValue(false);
            else if (this.commandChatTextBox.CurrentRecipientId != -1)
                this.commandChatTextBox.UpdateForNewRecepient(-1);
            else if (this.commandChatTextBox.finalText.Any())
            {
                this.commandChatTextBox.Reset();
                while (this.bCheatHistoryPosition.GetValue() != -1)
                    this.receiveKeyPress(Keys.Down);
            }
            else
                this.Reset();
        }

        private void Reset()
        {
            int old = this.bCheatHistoryPosition.GetValue();
            this.bCheatHistoryPosition.SetValue(-5);
            base.clickAway();

            if (this.bCheatHistoryPosition.GetValue() != -5)
            {
                this.displayLineIndex = this.bMessages.Count - 1;
                this.commandChatTextBox.Reset();
            }
            else
            {
                this.bCheatHistoryPosition.SetValue(old);
            }
        }

        /// <summary>Whether the given position is within the bounds of this menu.</summary>
        public override bool isWithinBounds(int x, int y)
        {
            if (x - this.xPositionOnScreen < this.width && x - this.xPositionOnScreen >= 0 &&
                y - this.yPositionOnScreen < this.height &&
                y - this.yPositionOnScreen >= -this.GetOldMessagesBoxHeight())
                return true;
            return this.bChoosingEmoji.GetValue() && this.emojiMenu.isWithinBounds(x, y);
        }

        /// <summary>Gets the old height of the chat box.</summary>
        private int GetOldMessagesBoxHeight()
        {
            return this.GetDisplayedLines().Select(item => item.verticalSize).Sum() + 20;
        }

        /// <summary>Adds a message with formatting to the chat box.</summary>
        public override void receiveChatMessage(long sourceFarmer, int chatKind,
            LocalizedContentManager.LanguageCode language, string message)
        {
            bool whisperMessage = false;
            string person = "you";
            if (message[0] == WhisperSeparator)
            {
                string[] parts = message.Substring(1).Split(WhisperSeparator);
                string recipientId = Utils.DecipherText(parts[0], sourceFarmer ^ Game1.player.UniqueMultiplayerID);
                if (!long.TryParse(recipientId, out long recId) || recId != Game1.player.UniqueMultiplayerID)
                    return;
                message = Utils.DecipherText(parts[1], sourceFarmer ^ Game1.player.UniqueMultiplayerID);
                whisperMessage = true;

                //check this tomorrow
                this.commandChatTextBox.LastWhisperId = sourceFarmer;
            }
            else if (message[0] == WhisperSeparator + 1)
            {
                string[] parts = message.Substring(1).Split(WhisperSeparator);
                person = parts[0];
                message = parts[1];
                whisperMessage = true;
            }

            string text1 = this.bFormatMessage.Invoke<string>(sourceFarmer, chatKind, message);
            if (whisperMessage)
                text1 = text1.Substring(0, text1.IndexOf(":", StringComparison.InvariantCultureIgnoreCase)) +
                        $" (to {person} only):" +
                        text1.Substring(text1.IndexOf(":", StringComparison.InvariantCultureIgnoreCase) + 1);
            string text2 = FixedParseText(text1, this.chatBox.Font, this.chatBox.Width - 16);

            foreach (string part in text2.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                this.AddNewMessage(part, this.messageColor(chatKind), this.chatBox.Font, language);
        }

        /// <summary>Adds a message without any formatting to the chat box.</summary>
        public void AddConsoleMessage(string message, Color color)
        {
            string text = FixedParseText(message, this.chatBox.Font, this.chatBox.Width - 8,
                this.config.UseMonospacedFontForCommandOutput);

            foreach (string part in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                this.AddNewMessage(part, color, this.chatBox.Font, LocalizedContentManager.CurrentLanguageCode, true);
        }

        /// <summary>Adds a message without any formatting to the chat box.</summary>
        public override void addMessage(string message, Color color)
        {
            string text = FixedParseText(message, this.chatBox.Font, this.chatBox.Width - 8);

            foreach (string part in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                this.AddNewMessage(part, color, this.chatBox.Font, LocalizedContentManager.CurrentLanguageCode);
        }

        /// <summary>Adds a new message to the chat box.</summary>
        private void AddNewMessage(string message, Color color, SpriteFont font,
            LocalizedContentManager.LanguageCode code, bool isConsoleMessage = false)
        {
            if (string.IsNullOrEmpty(message))
                message = " ";

            ChatMessage newMessage = isConsoleMessage && this.config.UseMonospacedFontForCommandOutput
                ? new ConsoleChatMessage()
                : new ChatMessage();

            newMessage.timeLeftToDisplay = 600;
            newMessage.verticalSize = (int)font.MeasureString(message).Y + 4;
            newMessage.color = color;
            newMessage.language = code;

            newMessage.parseMessageForEmoji(message);
            this.bMessages.Add(newMessage);
            if (this.config.MaximumNumberOfHistoryMessages > 0 &&
                this.bMessages.Count >= this.config.MaximumNumberOfHistoryMessages)
                this.bMessages.RemoveAt(0);
            else if (this.displayLineIndex == this.bMessages.Count - 2) this.displayLineIndex++;
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

        /// <summary>Handle the game window size changing.</summary>
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);

            this.DetermineNumberOfMaxMessages();
        }

        /// <summary>Determines the number of messages that should be displayed.</summary>
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

        /// <summary>Runs the given command.</summary>
        protected override void runCommand(string command)
        {
            base.runCommand(command);
            if (this.bMessages.Count <= this.displayLineIndex)
                this.displayLineIndex = this.bMessages.Count - 1;
        }

        /// <summary>Handles left click events.</summary>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!this.chatBox.Selected)
                return;

            if (this.bEmojiMenuIcon.containsPoint(x, y))
            {
                this.bChoosingEmoji.SetValue(!this.bChoosingEmoji.GetValue());
                Game1.playSound("shwip");
                this.bEmojiMenuIcon.scale = 4f;
            }
            else if (this.bChoosingEmoji.GetValue() && this.emojiMenu.isWithinBounds(x, y))
            {
                (this.emojiMenu as CommandEmojiMenu)?.LeftClick(x, y, this);
            }
            else
            {
                this.chatBox.Update();
                if (this.bChoosingEmoji.GetValue())
                {
                    this.bChoosingEmoji.SetValue(false);
                    this.bEmojiMenuIcon.scale = 4f;
                }

                if (!this.isWithinBounds(x, y))
                    return;
                this.chatBox.Selected = true;
            }
        }

        /// <summary>Handles key press events.</summary>
        public override void receiveKeyPress(Keys key)
        {
            if (!this.isActive())
                return;

            if (key == Keys.Up)
            {
                if (this.bCheatHistoryPosition.GetValue() >= this.sentMessageHistory.Count - 1)
                    return;

                if (this.bCheatHistoryPosition.GetValue() == -1)
                    this.currentTypedMessage = this.commandChatTextBox.Save();

                this.bCheatHistoryPosition.SetValue(this.bCheatHistoryPosition.GetValue() + 1);
                this.commandChatTextBox.Load(this.sentMessageHistory[this.bCheatHistoryPosition.GetValue()]);
            }
            else if (key == Keys.Down)
            {
                if (this.bCheatHistoryPosition.GetValue() <= 0)
                {
                    if (this.bCheatHistoryPosition.GetValue() == -1) return;
                    this.bCheatHistoryPosition.SetValue(-1);
                    //This makes the user able to type in the box again.
                    int currIndex = this.displayLineIndex;
                    this.clickAway();
                    this.activate();
                    this.displayLineIndex = currIndex;
                    this.commandChatTextBox.Load(this.currentTypedMessage, true);

                    return;
                }

                this.bCheatHistoryPosition.SetValue(this.bCheatHistoryPosition.GetValue() - 1);
                this.commandChatTextBox.Load(this.sentMessageHistory[this.bCheatHistoryPosition.GetValue()]);
            }
            else if (key == Keys.Left)
            {
                this.commandChatTextBox.OnLeftArrowPress();
            }
            else if (key == Keys.Right)
            {
                this.commandChatTextBox.OnRightArrowPress();
            }
        }

        /// <summary>Scrolls down one line.</summary>
        private void ScrollDown()
        {
            if (this.displayLineIndex >= this.bMessages.Count - 1) return;

            this.displayLineIndex++;
        }

        /// <summary>Scrolls up one line.</summary>
        private void ScrollUp()
        {
            if (this.displayLineIndex <= Math.Min(this.maxMessages, this.bMessages.Count) - 1) return;

            this.displayLineIndex--;
        }

        /// <summary>Handle scroll wheel events.</summary>
        public override void receiveScrollWheelAction(int direction)
        {
            if (!this.bChoosingEmoji.GetValue())
            {
                if (direction < 0)
                    this.ScrollDown();
                else
                    this.ScrollUp();
            }
            else
            {
                this.emojiMenu.receiveScrollWheelAction(direction);
            }
        }

        /// <summary>Draws the chat box and all its components.</summary>
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
                                    (this.bMessages.Count - this.displayLineIndex) * barWindowHeight / numScrolls;

                    int topOffset = 7 - 20 + windowHeight -
                                    (this.bMessages.Count - this.maxMessages + 1) * windowHeight / numScrolls;

                    int bottomOffset = 7 - 20 + windowHeight -
                                       (this.bMessages.Count - this.bMessages.Count) * windowHeight / numScrolls;
                    //bottom arrow
                    b.Draw(Game1.mouseCursors, new Vector2(this.xPositionOnScreen + this.width - 32 - 3,
                            this.yPositionOnScreen - windowHeight + (this.chatBox.Selected ? 0 : this.chatBox.Height) +
                            bottomOffset - 24),
                        new Rectangle(421, 472, 11, 12), Color.White, 0, Vector2.Zero, Vector2.One * 2,
                        SpriteEffects.None, 0);
                    //top arrow
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

            //messages themselves
            int num2 = 0;
            foreach (ChatMessage message in this.GetDisplayedLines())
            {
                num2 += message.verticalSize;
                if (message is ConsoleChatMessage cMessage)
                    cMessage.ConsoleDraw(b, 12,
                        this.yPositionOnScreen - num2 - 8 + (this.chatBox.Selected ? 0 : this.chatBox.Height));
                else
                    message.draw(b, 12,
                        this.yPositionOnScreen - num2 - 8 + (this.chatBox.Selected ? 0 : this.chatBox.Height));
            }

            if (!this.isActive())
                return;

            //text entry
            this.chatBox.Draw(b, false);

            //We need to draw non-numbers at half size, because they weren't made tiny when CA made the tiny font
            if (this.bCheatHistoryPosition.GetValue() != -1)
            {
                float x = this.chatBox.X;
                b.DrawString(Game1.tinyFont,
                    "#",
                    new Vector2(x, this.chatBox.Y), Color.White, 0f, Vector2.Zero, Vector2.One * 0.5f, SpriteEffects.None, 0);
                x += Game1.tinyFont.MeasureString("#").X * 0.5f;
                b.DrawString(Game1.tinyFont,
                    $"{this.bCheatHistoryPosition.GetValue() + 1}",
                    new Vector2(x, this.chatBox.Y - 12), Color.White);
                x += Game1.tinyFont.MeasureString($"{this.bCheatHistoryPosition.GetValue() + 1}").X;
                b.DrawString(Game1.tinyFont,
                    "/",
                    new Vector2(x, this.chatBox.Y), Color.White, 0f, Vector2.Zero, Vector2.One * 0.5f, SpriteEffects.None, 0);
                x += Game1.tinyFont.MeasureString("/").X * 0.5f;
                b.DrawString(Game1.tinyFont,
                    $"{this.sentMessageHistory.Count}",
                    new Vector2(x, this.chatBox.Y - 12), Color.White);
            }

            //emoticon icon
            this.bEmojiMenuIcon.draw(b, Color.White, 0.99f);
            if (!this.bChoosingEmoji.GetValue())
                return;
            //emoticon list
            this.emojiMenu.draw(b);
        }

        /// <summary>Clears sent message history.</summary>
        internal void ClearHistory()
        {
            this.sentMessageHistory.Clear();
            this.bCheatHistoryPosition.SetValue(-1);
            if (this.currentTypedMessage == null)
                this.currentTypedMessage = this.commandChatTextBox.Save();
            this.commandChatTextBox.Load(this.currentTypedMessage, true);
        }

        /// <summary>Gets all lines currently being displayed.</summary>
        private IEnumerable<ChatMessage> GetDisplayedLines()
        {
            for (int i = this.displayLineIndex; i >= this.GetEndDisplayIndex(); i--)
            {
                ChatMessage message = this.bMessages[i];
                if (this.chatBox.Selected || message.alpha > 0.00999999977648258)
                    yield return message;
            }
        }

        /// <summary>Gets the last index of the item that should be displayed.</summary>
        private int GetEndDisplayIndex()
        {
            if (this.displayLineIndex < this.maxMessages)
                return 0;

            return this.displayLineIndex - this.maxMessages + 1;
        }

        /// <summary>A fixed version of <see cref="Game1.parseText(string,SpriteFont,int)" /> that uses .X instead of .Length.</summary>
        private static string FixedParseText(string text, SpriteFont whichFont, int width, bool isConsole = false)
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
                    if (ConsoleChatMessage.MeasureStringWidth(whichFont, str1 + ch, isConsole) > (double)width)
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
                    if (ConsoleChatMessage.MeasureStringWidth(whichFont, str1 + str4, isConsole) > (double)width ||
                        str4.Equals(Environment.NewLine))
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
