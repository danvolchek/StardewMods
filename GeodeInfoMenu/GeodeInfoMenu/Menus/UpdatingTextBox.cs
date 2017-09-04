using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GeodeInfoMenu.Menus.SearchTab;
using StardewValley;
using Microsoft.Xna.Framework.Input;

namespace GeodeInfoMenu.Menus
{
    /// <summary>
    /// Represents a text box that calls a method every time its text is changed.
    /// </summary>
    public class UpdatingTextBox : IKeyboardSubscriber
    {
        /***
         * Existing Fields
         ***/
        public int textLimit = -1;
        public bool limitWidth = true;
        private string _text = "";
        private Texture2D _textBoxTexture;
        private Texture2D _caretTexture;
        private SpriteFont _font;
        private Color _textColor;
        public bool numbersOnly;
        private bool _showKeyboard;
        private bool _selected;

        /***
         * Changes from orignal class
         ***/
        public string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                this._text = value;
                if (this._text == null)
                    this._text = "";
                if (this._text == "")
                {
                    this.callback();
                    return;
                }
                string words = "";
                foreach (char ch in value)
                {
                    if (this._font.Characters.Contains(ch))
                        words += ch.ToString();
                }
                this._text = Program.sdk.FilterDirtyWords(words);
                if (!(!this.limitWidth || (double)this._font.MeasureString(this._text).X <= (double)(this.Width - Game1.tileSize / 3)))
                {
                    this.Text = this._text.Substring(0, this._text.Length - 1);
                }
                else
                    this.callback();
            }
        }

        private TextChangedDelegate callback;

        /***
         * Existing Properties
         ***/
        public SpriteFont Font
        {
            get
            {
                return this._font;
            }
        }

        public Color TextColor
        {
            get
            {
                return this._textColor;
            }
        }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool PasswordBox { get; set; }

        public string TitleText { get; set; }

        public bool Selected
        {
            get
            {
                return this._selected;
            }
            set
            {
                if (this._selected == value)
                    return;
                Console.WriteLine("TextBox.Selected is now '{0}'.", (object)value);
                this._selected = value;
                if (this._selected)
                {
                    Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)this;
                    this._showKeyboard = true;
                }
                else
                    this._showKeyboard = false;
            }
        }

        /***
         * Existing Methods
         ***/

        public event UpdatingTextBoxEvent OnEnterPressed;

        public event UpdatingTextBoxEvent OnTabPressed;

        public delegate void UpdatingTextBoxEvent(UpdatingTextBox sender);

        public UpdatingTextBox(TextChangedDelegate callback, Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor)
        {
            this._textBoxTexture = textBoxTexture;
            this.callback = callback;
            if (textBoxTexture != null)
            {
                this.Width = textBoxTexture.Width;
                this.Height = textBoxTexture.Height;
            }
            this._caretTexture = caretTexture;
            this._font = font;
            this._textColor = textColor;
        }

        public void SelectMe()
        {
            this.Selected = true;
        }

        public void Update()
        {
            Mouse.GetState();
            bool newSelected = new Rectangle(this.X, this.Y, this.Width, this.Height).Contains(new Point(Game1.getMouseX(), Game1.getMouseY()));
            if (this.Selected != newSelected)
                Console.WriteLine(newSelected);
            this.Selected = newSelected;
            if (!this._showKeyboard)
                return;
            this._showKeyboard = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            bool flag = DateTime.Now.Millisecond % 1000 >= 500;
            string text = this.Text;
            if (this.PasswordBox)
            {
                text = "";
                for (int index = 0; index < this.Text.Length; ++index)
                    text += "•";
            }
            if (this._textBoxTexture != null)
            {
                spriteBatch.Draw(this._textBoxTexture, new Rectangle(this.X, this.Y, Game1.tileSize / 4, this.Height), new Rectangle?(new Rectangle(0, 0, Game1.tileSize / 4, this.Height)), Color.White);
                spriteBatch.Draw(this._textBoxTexture, new Rectangle(this.X + Game1.tileSize / 4, this.Y, this.Width - Game1.tileSize / 2, this.Height), new Rectangle?(new Rectangle(Game1.tileSize / 4, 0, 4, this.Height)), Color.White);
                spriteBatch.Draw(this._textBoxTexture, new Rectangle(this.X + this.Width - Game1.tileSize / 4, this.Y, Game1.tileSize / 4, this.Height), new Rectangle?(new Rectangle(this._textBoxTexture.Bounds.Width - Game1.tileSize / 4, 0, Game1.tileSize / 4, this.Height)), Color.White);
            }
            else
                Game1.drawDialogueBox(this.X - Game1.tileSize / 2, this.Y - Game1.tileSize * 7 / 4 + 10, this.Width + Game1.tileSize * 5 / 4, this.Height, false, true, (string)null, false);
            Vector2 vector2;
            for (vector2 = this._font.MeasureString(text); (double)vector2.X > (double)this.Width; vector2 = this._font.MeasureString(text))
                text = text.Substring(1);
            Utility.drawTextWithShadow(spriteBatch, text, this._font, new Vector2((float)(this.X + Game1.tileSize / 4), (float)(this.Y + (this._textBoxTexture != null ? Game1.tileSize / 4 - Game1.pixelZoom : Game1.pixelZoom * 2))), this._textColor, 1f, -1f, -1, -1, 1f, 3);
            if (flag && this.Selected)
                spriteBatch.Draw(Game1.staminaRect, new Rectangle(this.X + Game1.tileSize / 4 + (int)vector2.X + 2, this.Y + 8, 4, 32), this._textColor);

        }

        public void RecieveTextInput(char inputChar)
        {
            if (!this.Selected || this.numbersOnly && !char.IsDigit(inputChar) || this.textLimit != -1 && this.Text.Length >= this.textLimit)
                return;
            if ((int)Game1.gameMode != 3)
            {
                if ((uint)inputChar <= 42U)
                {
                    if ((int)inputChar == 34)
                        return;
                    if ((int)inputChar != 36)
                    {
                        if ((int)inputChar == 42)
                        {
                            Game1.playSound("hammer");
                            goto label_17;
                        }
                    }
                    else
                    {
                        Game1.playSound("money");
                        goto label_17;
                    }
                }
                else if ((int)inputChar != 43)
                {
                    if ((int)inputChar != 60)
                    {
                        if ((int)inputChar == 61)
                        {
                            Game1.playSound("coin");
                            goto label_17;
                        }
                    }
                    else
                    {
                        Game1.playSound("crystal");
                        goto label_17;
                    }
                }
                else
                {
                    Game1.playSound("slimeHit");
                    goto label_17;
                }
                Game1.playSound("cowboy_monsterhit");
            }
            label_17:
            this.Text = this.Text + inputChar.ToString();
        }

        public void RecieveTextInput(string text)
        {
            int result = -1;
            if (!this.Selected || this.numbersOnly && !int.TryParse(text, out result) || this.textLimit != -1 && this.Text.Length >= this.textLimit)
                return;
            this.Text = this.Text + text;
        }

        public void RecieveCommandInput(char command)
        {
            if (!this.Selected)
                return;
            if ((int)command != 8)
            {
                if ((int)command != 9)
                {
                    // ISSUE: reference to a compiler-generated field
                    if ((int)command != 13 || this.OnEnterPressed == null)
                        return;
                    // ISSUE: reference to a compiler-generated field
                    this.OnEnterPressed(this);
                }
                else
                {
                    // ISSUE: reference to a compiler-generated field
                    if (this.OnTabPressed == null)
                        return;
                    // ISSUE: reference to a compiler-generated field
                    this.OnTabPressed(this);
                }
            }
            else
            {
                if (this.Text.Length <= 0)
                    return;
                this.Text = this.Text.Substring(0, this.Text.Length - 1);
                if ((int)Game1.gameMode == 3)
                    return;
                Game1.playSound("tinyWhip");
            }
        }

        public void RecieveSpecialInput(Keys key)
        {
        }

        public void Hover(int x, int y)
        {
            if (x <= this.X || x >= this.X + this.Width || (y <= this.Y || y >= this.Y + this.Height))
                return;
            Game1.SetFreeCursorDrag();
        }
    }
}
