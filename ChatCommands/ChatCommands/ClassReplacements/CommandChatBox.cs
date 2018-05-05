using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ChatCommands.ClassReplacements
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
        private readonly CommandChatTextBox commandChatTextBox;
        private readonly int maxHistoryEntries;
        private int displayLineIndex;

        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <remarks>Reassigns the enter handler, replaces <see cref="ChatTextBox"/> and <see cref="EmojiMenu"/>.</remarks>
        public CommandChatBox(IReflectionHelper helper, ICommandHandler handler, ChatCommandsConfig config)
        {
            this.handler = handler;
            this.bCheatHistoryPosition = helper.GetField<int>(this, "cheatHistoryPosition");
            this.bFormatMessage = helper.GetMethod(this, "formatMessage");
            this.bMessages = helper.GetField<List<ChatMessage>>(this, "messages").GetValue();
            this.bCheatHistory = helper.GetField<List<string>>(this, "cheatHistory").GetValue();
            this.bEmojiMenuIcon = helper.GetField<ClickableTextureComponent>(this, "emojiMenuIcon").GetValue();
            this.bChoosingEmoji = helper.GetField<bool>(this, "choosingEmoji");
            Texture2D chatBoxTexture = Game1.content.Load<Texture2D>("LooseSprites\\chatBox");

            this.chatBox.OnEnterPressed -= helper.GetField<TextBoxEvent>(this, "e").GetValue();
            this.chatBox = this.commandChatTextBox = new CommandChatTextBox(chatBoxTexture,
                null, Game1.smallFont, Color.White);
            Game1.keyboardDispatcher.Subscriber = this.chatBox;
            this.chatBox.Selected = false;
            this.chatBox.OnEnterPressed += this.EnterPressed;

            this.emojiMenu = new CommandEmojiMenu(helper, this, emojiTexture, chatBoxTexture);

            helper.GetMethod(this, "updatePosition").Invoke();


            this.displayLineIndex = -1;
            this.maxHistoryEntries = config.MaximumNumberOfHistoryMessages;
            this.DetermineNumberOfMaxMessages();
        }

        /// <summary>
        /// Handle enter being pressed.
        /// </summary>
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

                this.commandChatTextBox.Reset();
                this.bCheatHistoryPosition.SetValue(-1);
            }

            sender.Text = "";
            this.clickAway();
        }

        /*private bool ignoringClickAways;

        public override void clickAway()
        {
            if (this.ignoringClickAways)
            {
                if (!this.gameInputState.GetKeyboardState().IsKeyDown(Keys.Escape))
                {
                    this.ignoringClickAways = false;
                }
            } else if (this.bChoosingEmoji.GetValue())
            {
                this.bChoosingEmoji.SetValue(false);
                if (this.gameInputState.GetKeyboardState().IsKeyDown(Keys.Escape))
                    this.ignoringClickAways = true;
            }
            else
            {
                this.chatBox.Selected = false;
                this.setText("");
                this.bCheatHistoryPosition.SetValue(-1);
                this.displayLineIndex = this.bMessages.Count - 1;
                this.commandChatTextBox.Reset();
            }
        }*/

        /// <summary>
        /// Handles deactiving the chat box.
        /// </summary>
        public override void clickAway()
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
                this.bCheatHistoryPosition.SetValue(old);

        }

        /// <summary>
        /// Whether the given position is within the bounds of this menu.
        /// </summary>
        public override bool isWithinBounds(int x, int y)
        {
            if (x - this.xPositionOnScreen < this.width && x - this.xPositionOnScreen >= 0 &&
                y - this.yPositionOnScreen < this.height &&
                y - this.yPositionOnScreen >= -this.GetOldMessagesBoxHeight())
                return true;
            return this.bChoosingEmoji.GetValue() && this.emojiMenu.isWithinBounds(x, y);
        }

        /// <summary>
        /// Gets the old height of the chat box.
        /// </summary>
        private int GetOldMessagesBoxHeight()
        {
            return this.GetDisplayedLines().Select(item => item.verticalSize).Sum() + 20;
        }

        /// <summary>
        /// Adds a message with formatting to the chat box.
        /// </summary>
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

        /// <summary>
        /// Adds a message without any formatting to the chat box.
        /// </summary>
        public override void addMessage(string message, Color color)
        {
            string text = FixedParseText(message, this.chatBox.Font, this.chatBox.Width - 8);

            foreach (string part in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                this.AddNewMessage(part, color, LocalizedContentManager.CurrentLanguageCode);
            }
        }

        /// <summary>
        /// Adds a new message to the chat box.
        /// </summary>
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
            else if (this.displayLineIndex == this.bMessages.Count - 2) this.displayLineIndex++;
        }

        /// <summary>
        /// Resets the chatbox with the given literal text, not handling any emojis.
        /// </summary>
        /// <remarks>Doing it in this roundabout manner makes it so that the player can keep typing.</remarks>
        private void SimulatePlayerReset(string input)
        {
            this.commandChatTextBox.MoveCursorAllTheWayRight();
            this.commandChatTextBox.Backspace();
            while (!this.IsChatBoxEmpty())
                this.commandChatTextBox.Backspace();
            foreach (char c in input.Trim())
                this.chatBox.RecieveTextInput(c);
        }

        /// <summary>
        /// Whether the chat box is empty or not.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Handle the game window size changing.
        /// </summary>
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);

            this.DetermineNumberOfMaxMessages();
        }

        /// <summary>
        /// Determins the number of messages that should be displayed.
        /// </summary>
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

        /// <summary>
        /// Runs the given command.
        /// </summary>
        protected override void runCommand(string command)
        {
            base.runCommand(command);
            if (this.bMessages.Count <= this.displayLineIndex)
                this.displayLineIndex = this.bMessages.Count - 1;
        }

        /// <summary>
        /// Handles left click events.
        /// </summary>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.bEmojiMenuIcon.containsPoint(x, y))
            {
                this.bChoosingEmoji.SetValue(!this.bChoosingEmoji.GetValue());
                Game1.playSound("shwip");
                this.bEmojiMenuIcon.scale = 4f;
            }
            else if (this.emojiMenu.isWithinBounds(x, y))
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

        /// <summary>
        /// Handles key press events.
        /// </summary>
        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Up)
            {
                if (this.bCheatHistoryPosition.GetValue() >= this.bCheatHistory.Count - 1)
                    return;

                if (this.bCheatHistoryPosition.GetValue() == -1)
                {
                    this.commandChatTextBox.Save();
                }

                this.bCheatHistoryPosition.SetValue(this.bCheatHistoryPosition.GetValue() + 1);
                this.SimulatePlayerReset(this.bCheatHistory[this.bCheatHistoryPosition.GetValue()]);
            }
            else if (key == Keys.Down)
            {
                if (this.bCheatHistoryPosition.GetValue() <= 0)
                {
                    if (this.bCheatHistoryPosition.GetValue() != 0) return;
                    //This makes the user able to type in the box again.
                    int currIndex = this.displayLineIndex;
                    this.clickAway();
                    this.activate();
                    this.displayLineIndex = currIndex;
                    this.commandChatTextBox.Load();

                    return;
                }

                this.bCheatHistoryPosition.SetValue(this.bCheatHistoryPosition.GetValue() - 1);
                this.SimulatePlayerReset(this.bCheatHistory[this.bCheatHistoryPosition.GetValue()]);
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

        /// <summary>
        /// Scrolls down one line.
        /// </summary>
        private void ScrollDown()
        {
            if (this.displayLineIndex >= this.bMessages.Count - 1) return;

            this.displayLineIndex++;
        }

        /// <summary>
        /// Scrolls up one line.
        /// </summary>
        private void ScrollUp()
        {
            if (this.displayLineIndex <= Math.Min(this.maxMessages, this.bMessages.Count) - 1) return;

            this.displayLineIndex--;
        }

        /// <summary>
        /// Handle scroll wheel events.
        /// </summary>
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
                this.emojiMenu.receiveScrollWheelAction(direction);
        }

        /// <summary>
        /// Draws the chat box and all its components.
        /// </summary>
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
                                    (this.bMessages.Count - this.maxMessages + 1) * (windowHeight) / numScrolls;

                    int bottomOffset = 7 - 20 + windowHeight -
                                    (this.bMessages.Count - (this.bMessages.Count)) * (windowHeight) / numScrolls;
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
                message.draw(b, 12,
                    this.yPositionOnScreen - num2 - 8 + (this.chatBox.Selected ? 0 : this.chatBox.Height));
            }

            if (!this.chatBox.Selected)
                return;
            //text entry
            this.chatBox.Draw(b, false);
            //emoticon icon
            this.bEmojiMenuIcon.draw(b, Color.White, 0.99f);
            if (!this.bChoosingEmoji.GetValue())
                return;
            //emoticon list
            this.emojiMenu.draw(b);
        }

        /// <summary>
        /// Gets all lines currently being displayed.
        /// </summary>
        private IEnumerable<ChatMessage> GetDisplayedLines()
        {
            for (int i = this.displayLineIndex; i >= this.GetEndDisplayIndex(); i--)
            {
                ChatMessage message = this.bMessages[i];
                if (this.chatBox.Selected || message.alpha > 0.00999999977648258)
                    yield return message;
            }
        }

        /// <summary>
        /// Gets the last index of the item that should be displayed.
        /// </summary>
        private int GetEndDisplayIndex()
        {
            if (this.displayLineIndex < this.maxMessages)
                return 0;

            return this.displayLineIndex - this.maxMessages + 1;
        }

        /// <summary>
        /// A fixed version of <see cref="Game1.parseText(string,Microsoft.Xna.Framework.Graphics.SpriteFont,int)"/> that uses .X instead of .Length.
        /// </summary>
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