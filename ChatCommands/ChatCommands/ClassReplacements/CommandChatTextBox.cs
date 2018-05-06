using System;
using System.Collections.Generic;
using System.Linq;
using ChatCommands.Util;
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
                //The current snippet is an emoji
                if (this.currentInsertPosition == 0) //Create a new text snippet before this one.
                {
                    this.finalText.Insert(this.currentSnippetIndex, new ChatSnippet(text, LocalizedContentManager.CurrentLanguageCode));
                }
                else //Create a new text snippet after this one.
                {
                    this.finalText.Insert(this.currentSnippetIndex + 1, new ChatSnippet(text, LocalizedContentManager.CurrentLanguageCode));
                    this.currentSnippetIndex++;
                }
                this.currentInsertPosition = this.GetLastIndexOfCurrentSnippet();

            }
            else
            {
                //The current snippet is a text snippet, add text to it.
                ChatSnippet currSnippet = this.finalText[this.currentSnippetIndex];
                currSnippet.message = currSnippet.message.Substring(0, this.currentInsertPosition) + text +
                                      currSnippet.message.Substring(this.currentInsertPosition);
                this.currentInsertPosition += text.Length;

                ReMeasureSnippetLength(currSnippet);
            }

            this.updateWidth();
        }
        //AXIOMS:
        //If the current message is text, this.currentInsertPosition is in [0,text.length]
        //If the current message is an emoji, this.currentInsertPosition is either 0 or 1 (before/after)

        /// <summary>
        /// Handle the backspace key being pressed.
        /// </summary>
        public void Backspace()
        {
            if (this.finalText.Any())
            {
                if (this.currentInsertPosition == 0)
                {
                    //The current snippet is the first one.
                    if (this.currentSnippetIndex == 0)
                        return;

                    if (this.finalText[this.currentSnippetIndex].message == null)
                    {
                        //The current snippet is an emoji,
                        //so the before this one is either text or an emoji
                        ChatSnippet lastSnippet = this.finalText[this.currentSnippetIndex - 1];

                        if (lastSnippet.message == null)
                        {
                            //The previous snippet is an emoji.
                            //But the current snippet is also an emoji, so no merging - just delete it.
                            this.finalText.RemoveAt(this.currentSnippetIndex - 1);
                            this.currentSnippetIndex--;
                        }
                        else
                        {
                            //The previous snippet is text, delete a character from the end of it,
                            //and remove it if necessary.
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
                        //The current snippet is a text snippet.

                        //There must be an emoji before this snippet, remove it.
                        this.finalText.RemoveAt(this.currentSnippetIndex - 1);
                        this.currentSnippetIndex--;

                        //This snippet now needs to merge with the previous snippet,
                        //which may not exist (no merging), or may be an emoji (no merging).

                        //No previous snippet.
                        if (this.currentSnippetIndex == 0)
                            return;
                        //Previous snippet is an emoji.
                        if (this.finalText[this.currentSnippetIndex - 1].message == null)
                            return;

                        //Merge the current snippet with the previous one.
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
                    //The current snippet is an emoji, and the insert position has to be one,
                    //so we delete the emoji.
                    this.finalText.RemoveAt(this.currentSnippetIndex);
                    if (this.currentSnippetIndex != 0)
                    {
                        //The removed emoji was not the first snippet.
                        this.currentSnippetIndex--;
                        this.currentInsertPosition = this.GetLastIndexOfCurrentSnippet();

                        //If this emoji seperated two text snippets, they need to be
                        //merged.
                        if (this.currentSnippetIndex != this.finalText.Count - 1 &&
                            this.finalText[this.currentSnippetIndex + 1].message != null)
                        {
                            //Merge the next and current text snippets.
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
                    //The current snippet is a text snippet, and the current insert position
                    //is not at the start of the it, so a character is removed from it.
                    ChatSnippet currSnippet = this.finalText[this.currentSnippetIndex];
                    currSnippet.message = currSnippet.message.Remove(this.currentInsertPosition - 1, 1);

                    if (currSnippet.message.Length == 0)
                    {
                        //If the entire snippet is now empty, remove it.
                        this.finalText.Remove(currSnippet);
                        if (this.currentSnippetIndex != 0)
                        {
                            this.currentSnippetIndex--;
                            this.currentInsertPosition = this.GetLastIndexOfCurrentSnippet();
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
        /// Gets the last insertion index of the current snippet.
        /// </summary>
        private int GetLastIndexOfCurrentSnippet()
        {
            return GetLastIndexOfMessage(this.finalText[this.currentSnippetIndex]);
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
                //Inserting at the start of a text or emoji snippet - add before the current snippet.
                this.finalText.Insert(this.currentSnippetIndex, new ChatSnippet(emoji));
                if (this.finalText.Count != 1)
                    this.currentSnippetIndex++;
                else
                    this.currentInsertPosition = this.GetLastIndexOfCurrentSnippet();
            }
            else if (this.currentInsertPosition == this.GetLastIndexOfCurrentSnippet())
            {
                //Inserting at the end of a text or emoji snippet message - add after current snippet.
                ChatSnippet emojiSnippet = new ChatSnippet(emoji);
                this.finalText.Insert(this.currentSnippetIndex + 1, emojiSnippet);
                this.currentSnippetIndex++;
                this.currentInsertPosition = GetLastIndexOfMessage(emojiSnippet);
            }
            else
            {
                //Inserting at the middle of a text message - split the message in two.
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
        
        /// <summary>
        /// Draws the chat text box.
        /// </summary>
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
            //Current position is at the boundry of a snippet.
            if (this.currentInsertPosition == 0)
            {
                //If there are no snippets before this, the cursor can't move any more left.
                if (this.currentSnippetIndex == 0)
                    return;

                //Move to previous snippet.
                this.currentSnippetIndex--;

                //Move to start if the current snippet is an emoji, or end - 1 if the current snippet
                //is a text snippet.
                this.currentInsertPosition = this.finalText[this.currentSnippetIndex].message == null ?
                    0 : (this.GetLastIndexOfCurrentSnippet() - 1);

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
            //Current position is at the boundry of a snippet.
            if (this.currentInsertPosition == (this.finalText[this.currentSnippetIndex].message?.Length ?? 1))
            {
                //If there are no snippets before this, the cursor can't move any more right.
                if (this.currentSnippetIndex == this.finalText.Count - 1)
                    return;

                //Move to next snippet.
                this.currentSnippetIndex++;

                //Move to the end of the next snippet if it is an emoji, or the first + 1 of the
                //next snippet if it is text. This is 1 in both cases.
                this.currentInsertPosition = 1;

            }
            else
                this.currentInsertPosition++;
        }

        /// <summary>
        /// Move the cursor as far right as possible.
        /// </summary>
        private void MoveCursorAllTheWayRight()
        {
            if (!this.finalText.Any())
                return;
            this.currentSnippetIndex = this.finalText.Count - 1;
            this.currentInsertPosition = this.GetLastIndexOfCurrentSnippet();
        }

        /// <summary>
        /// Save the current state, so it can be restored later.
        /// </summary>
        public CommandChatTextBoxState Save()
        {
            return new CommandChatTextBoxState(this.currentInsertPosition, this.currentSnippetIndex, this.finalText);
            
        }

        /// <summary>
        /// Restored a previously saved state.
        /// </summary>
        public void Load(CommandChatTextBoxState state, bool useSavedPosition = false)
        {
            this.finalText.Clear();
            foreach (ChatSnippet snippet in state.FinalText)
                this.finalText.Add(Utils.CopyChatSnippet(snippet));
            if (useSavedPosition)
            {
                this.currentInsertPosition = state.CurrentInsertPosition;
                this.currentSnippetIndex = state.CurrentSnippetIndex;
                
            }
            else
            {
                this.MoveCursorAllTheWayRight();
            }
            this.updateWidth();
        }
    }
}
