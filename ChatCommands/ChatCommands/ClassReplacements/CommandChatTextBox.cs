using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace ChatCommands.ClassReplacements
{
    internal class CommandChatTextBox : ChatTextBox
    {
        private int currentInsertPosition;
        private int currentSnippetIndex;

        private int lastUpdateInsertPosition;
        private int lastUpdatecurrentSnippetIndex;

        private int savedCurrentInsertPosition = -1;
        private int savedCurrentSnippetIndex = -1;
        private readonly List<ChatSnippet> savedFinalText = new List<ChatSnippet>();

        public CommandChatTextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor) : base(textBoxTexture, caretTexture, font, textColor)
        {
        }

        /// <summary>
        /// Reset the current state.
        /// </summary>
        public void Reset()
        {
            this.currentWidth = 0.0f;
            this.finalText.Clear();
            this.currentSnippetIndex = this.currentInsertPosition = 0;
        }

        /// <summary>
        /// Handle command input.
        /// </summary>
        public override void RecieveCommandInput(char command)
        {
            if (this.Selected && command == 8)
                this.Backspace();
            else
            {
                base.RecieveCommandInput(command);
                if (command == 13)
                {
                    this.currentInsertPosition = this.currentSnippetIndex = 0;
                }
            }
        }

        /// <summary>
        /// Add text to the text box.
        /// </summary>
        public override void RecieveTextInput(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (this.finalText.Count == 0)
                this.finalText.Add(new ChatSnippet("", LocalizedContentManager.CurrentLanguageCode));

            if ((double)this.currentWidth + ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode).MeasureString(text).X >= (this.Width - 16))
                return;

            if (this.finalText[this.currentSnippetIndex].message == null)
            {
                //we're in an emogi
                if (this.currentInsertPosition == 0) //start a new text snippet before this one
                {
                    this.finalText.Insert(this.currentSnippetIndex, new ChatSnippet(text, LocalizedContentManager.CurrentLanguageCode));
                }
                else //start a new snippet after this one
                {
                    this.finalText.Insert(this.currentSnippetIndex + 1, new ChatSnippet(text, LocalizedContentManager.CurrentLanguageCode));
                    this.currentSnippetIndex++;
                }
                this.currentInsertPosition = GetLastIndexOfMessage(this.finalText[this.currentSnippetIndex]);

            }
            else
            {
                //we're in a text box
                ChatSnippet currSnippet = this.finalText[this.currentSnippetIndex];
                currSnippet.message = currSnippet.message.Substring(0, this.currentInsertPosition) + text +
                                      currSnippet.message.Substring(this.currentInsertPosition);
                this.currentInsertPosition += text.Length;

                ReMeasureSnippetLength(currSnippet);
            }

            this.updateWidth();
        }
        //AXIOMS:
        //If the current message is text, we can be anywhere from [0,text.length]
        //If the current message is an emoji, we're at either 0 or 1 (before/after)

        /// <summary>
        /// Handle the backspace key being pressed.
        /// </summary>
        public void Backspace()
        {
            if (this.finalText.Any())
            {
                //either at the start, or an emoji before us
                if (this.currentInsertPosition == 0)
                {
                    //at the start
                    if (this.currentSnippetIndex == 0)
                        return;

                    if (this.finalText[this.currentSnippetIndex].message == null)
                    {
                        //we're an emoji, so the thing to the left of us is either text or an emoji
                        ChatSnippet lastSnippet = this.finalText[this.currentSnippetIndex - 1];

                        if (lastSnippet.message == null)
                        {
                            //the thing to the left of us is an emoji. But we're also an emoji, so no merging.
                            this.finalText.RemoveAt(this.currentSnippetIndex - 1);
                            this.currentSnippetIndex--;
                        }
                        else
                        {
                            lastSnippet.message = lastSnippet.message.Substring(0, lastSnippet.message.Length - 1);
                            if (lastSnippet.message.Length == 0)
                            {
                                this.finalText.Remove(lastSnippet);
                                this.currentSnippetIndex--;
                            }
                            else
                                ReMeasureSnippetLength(lastSnippet);
                        }


                    }
                    else
                    {
                        //must be an emoji to the left and we're text
                        this.finalText.RemoveAt(this.currentSnippetIndex - 1);
                        this.currentSnippetIndex--;

                        //need to merge with the previous snippet, of which there not may be one,
                        //and it may be an emoji

                        //now at the start
                        if (this.currentSnippetIndex == 0)
                            return;
                        //emoji before us
                        if (this.finalText[this.currentSnippetIndex - 1].message == null)
                            return;

                        //merge into the text before us
                        ChatSnippet last = this.finalText[this.currentSnippetIndex - 1];
                        this.currentInsertPosition = last.message.Length;
                        last.message += this.finalText[this.currentSnippetIndex].message;

                        ReMeasureSnippetLength(last);

                        this.finalText.RemoveAt(this.currentSnippetIndex);
                        this.currentSnippetIndex--;
                    }
                }
                else if (this.finalText[this.currentSnippetIndex].message == null)
                {
                    //we're in an emoji, and not at the start of the emoji. Bye bye emoji
                    this.finalText.RemoveAt(this.currentSnippetIndex);
                    if (this.currentSnippetIndex != 0)
                    {
                        this.currentSnippetIndex--;
                        this.currentInsertPosition = GetLastIndexOfMessage(this.finalText[this.currentSnippetIndex]);

                        //we may now need to merge two text boxes
                        if (this.currentSnippetIndex != this.finalText.Count - 1 &&
                            this.finalText[this.currentSnippetIndex + 1].message != null)
                        {
                            //merge into text after us
                            ChatSnippet next = this.finalText[this.currentSnippetIndex + 1];
                            ChatSnippet curr = this.finalText[this.currentSnippetIndex];
                            this.currentInsertPosition = curr.message.Length;
                            curr.message += next.message;

                            ReMeasureSnippetLength(curr);
                            this.finalText.Remove(next);
                        }
                    }
                    else
                        this.currentInsertPosition = 0;
                }
                else
                {
                    //we're in the middle of a text message
                    ChatSnippet currSnippet = this.finalText[this.currentSnippetIndex];
                    currSnippet.message = currSnippet.message.Remove(this.currentInsertPosition - 1, 1);
                    if (currSnippet.message.Length == 0)
                    {
                        //we deleted the entire message
                        this.finalText.Remove(currSnippet);
                        if (this.currentSnippetIndex != 0)
                        {
                            this.currentSnippetIndex--;
                            this.currentInsertPosition =
                                GetLastIndexOfMessage(this.finalText[this.currentSnippetIndex]);
                        }
                        else
                            this.currentInsertPosition = 0;
                    }
                    else
                    {
                        ReMeasureSnippetLength(currSnippet);
                        this.currentInsertPosition--;
                    }
                }
            }
            this.updateWidth();
        }

        /// <summary>
        /// Re measures the length of the given snippet.
        /// </summary>
        /// <param name="snippet"></param>
        private static void ReMeasureSnippetLength(ChatSnippet snippet)
        {
            if (snippet.message == null)
                snippet.myLength = 40;
            else
                snippet.myLength = ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode).MeasureString(snippet.message).X;

        }

        /// <summary>
        /// Gets the last insertion index of the given snippet.
        /// </summary>
        private static int GetLastIndexOfMessage(ChatSnippet snippet)
        {
            return snippet.message?.Length ?? 1;
        }

        /// <summary>
        /// Add an emoji to the typed text.
        /// </summary>
        public void ReceiveEmoji(int emoji)
        {
            if (this.currentWidth + 40.0 > (this.Width - 16))
                return;
            if (this.currentInsertPosition == 0)
            {
                //inserting at the start of a message - add before
                this.finalText.Insert(this.currentSnippetIndex, new ChatSnippet(emoji));
                this.currentSnippetIndex++;
            }
            else if (this.currentInsertPosition == this.finalText[this.currentSnippetIndex].message.Length - 1)
            {
                //inserting at the end of a message - add after
                ChatSnippet emojiSnippet = new ChatSnippet(emoji);
                this.finalText.Insert(this.currentSnippetIndex + 1, emojiSnippet);
                this.currentSnippetIndex++;
                this.currentInsertPosition = GetLastIndexOfMessage(emojiSnippet);
            }
            else
            {

                //inserting at the middle of a message - split
                ChatSnippet orig = this.finalText[this.currentSnippetIndex];

                string first = orig.message.Substring(0, this.currentInsertPosition);
                string second = orig.message.Substring(this.currentInsertPosition);

                this.finalText.RemoveAt(this.currentSnippetIndex);
                this.finalText.Insert(this.currentSnippetIndex, new ChatSnippet(second, LocalizedContentManager.CurrentLanguageCode));
                this.finalText.Insert(this.currentSnippetIndex, new ChatSnippet(emoji));
                this.finalText.Insert(this.currentSnippetIndex, new ChatSnippet(first, LocalizedContentManager.CurrentLanguageCode));
                this.currentSnippetIndex += 2;
                this.currentInsertPosition = 0;
            }
            this.updateWidth();
        }

        //take into account currents
        public override void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
        {
            if (this.Selected)
            {
                this.Selected = false;
                base.Draw(spriteBatch, drawShadow);
                this.Selected = true;

                bool forceDrawCursor = this.lastUpdateInsertPosition != this.currentInsertPosition ||
                    this.lastUpdatecurrentSnippetIndex != this.currentSnippetIndex;

                if (forceDrawCursor || DateTime.Now.Millisecond % 1000 >= 500)
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle(this.X + 16 + (int)this.GetCursorOffset() - 3, this.Y + 8, 4, 32), this._textColor);

                this.lastUpdatecurrentSnippetIndex = this.currentSnippetIndex;
                this.lastUpdateInsertPosition = this.currentInsertPosition;
            }
            else
                base.Draw(spriteBatch, drawShadow);

        }

        /// <summary>
        /// Gets the current offset of the cursor from the left of the textbox.
        /// </summary>
        private float GetCursorOffset()
        {
            float offset = 0;

            if (!this.finalText.Any())
                return offset;
            for (int i = 0; i < this.currentSnippetIndex; i++)
                offset += this.finalText[i].myLength;

            if (this.finalText[this.currentSnippetIndex].message == null)
                offset += this.currentInsertPosition == 1 ? 40 : 0;
            else
                offset += ChatBox.messageFont(LocalizedContentManager.CurrentLanguageCode)
                    .MeasureString(this.finalText[this.currentSnippetIndex].message
                        .Substring(0, this.currentInsertPosition)).X;
            return offset;
        }

        /// <summary>
        /// Handle the left arrow key being pressed.
        /// </summary>
        public void OnLeftArrowPress()
        {
            if (!this.finalText.Any())
                return;
            //we're at the boundry of a snippet
            if (this.currentInsertPosition == 0)
            {
                //if there are no snippets before this, we can't go left anymore
                if (this.currentSnippetIndex == 0)
                    return;

                //move to previous snippet
                this.currentSnippetIndex--;

                //move to start if we moved onto an emoji, or end - 1 if we moved onto text
                this.currentInsertPosition = this.finalText[this.currentSnippetIndex].message == null ?
                    0 : (GetLastIndexOfMessage(this.finalText[this.currentSnippetIndex]) - 1);

            }
            else
                this.currentInsertPosition--;
        }

        /// <summary>
        /// Handle the right arrow key being pressed.
        /// </summary>
        public void OnRightArrowPress()
        {
            if (!this.finalText.Any())
                return;
            //we're at the right boundary of a snippet
            if (this.currentInsertPosition == (this.finalText[this.currentSnippetIndex].message?.Length ?? 1))
            {
                //there are no more snippets to the right, so we can't go right any more
                if (this.currentSnippetIndex == this.finalText.Count - 1)
                    return;

                //move to next snippet
                this.currentSnippetIndex++;

                //move to the end of the emoji, or the first + 1 of text
                this.currentInsertPosition = 1;

            }
            else
                this.currentInsertPosition++;
        }

        /// <summary>
        /// Move the cursor as far right as possible.
        /// </summary>
        public void MoveCursorAllTheWayRight()
        {
            if (!this.finalText.Any())
                return;
            this.currentSnippetIndex = this.finalText.Count - 1;
            this.currentInsertPosition = GetLastIndexOfMessage(this.finalText[this.currentSnippetIndex]);
        }

        /// <summary>
        /// Save the current state, so it can be restored later.
        /// </summary>
        public void Save()
        {
            this.savedCurrentInsertPosition = this.currentInsertPosition;
            this.savedCurrentSnippetIndex = this.currentSnippetIndex;
            this.savedFinalText.Clear();
            foreach (ChatSnippet snippet in this.finalText)
            {
                this.savedFinalText.Add(snippet.message != null
                    ? new ChatSnippet(snippet.message, LocalizedContentManager.CurrentLanguageCode)
                    : new ChatSnippet(snippet.emojiIndex));
            }
        }

        /// <summary>
        /// Restored a previously saved state.
        /// </summary>
        public void Load()
        {
            this.currentInsertPosition = this.savedCurrentInsertPosition;
            this.currentSnippetIndex = this.savedCurrentSnippetIndex;
            this.finalText.Clear();
            foreach (ChatSnippet snippet in this.savedFinalText)
                this.finalText.Add(snippet);
            this.updateWidth();
        }
    }
}
