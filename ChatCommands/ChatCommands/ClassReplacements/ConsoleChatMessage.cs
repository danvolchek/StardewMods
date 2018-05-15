using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace ChatCommands.ClassReplacements
{
    /// <summary>
    ///     Draws the message using a monospaced font.
    /// </summary>
    internal class ConsoleChatMessage : ChatMessage
    {
        private static float widestCharacter = -1;

        public void ConsoleDraw(SpriteBatch b, int x, int y)
        {
            SpriteFont font = ChatBox.messageFont(this.language);
            float num1 = 0.0f;
            float num2 = 0.0f;
            for (int index = 0; index < this.message.Count; ++index)
            {
                if (this.message[index].emojiIndex != -1)
                {
                    b.Draw(ChatBox.emojiTexture,
                        new Vector2((float) (x + (double) num1 + 1.0), (float) (y + (double) num2 - 4.0)),
                        new Rectangle(this.message[index].emojiIndex * 9 % ChatBox.emojiTexture.Width,
                            this.message[index].emojiIndex * 9 / ChatBox.emojiTexture.Width * 9, 9, 9),
                        Color.White * this.alpha, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
                }
                else if (this.message[index].message != null)
                {
                    if (this.message[index].message.Equals(Environment.NewLine))
                    {
                        num1 = 0.0f;
                        num2 += font.MeasureString("(").Y;
                    }
                    else
                    {
                        DrawMonoSpacedString(b, font, this.message[index].message, new Vector2(x + num1, y + num2),
                            this.color * this.alpha, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
                    }
                }

                num1 += this.message[index].myLength;
                if (num1 >= 888.0)
                {
                    num1 = 0.0f;
                    num2 += font.MeasureString("(").Y;
                    if (this.message.Count > index + 1 && this.message[index + 1].message != null &&
                        this.message[index + 1].message.Equals(Environment.NewLine))
                        ++index;
                }
            }
        }

        private static void DrawMonoSpacedString(SpriteBatch b, SpriteFont font, string text, Vector2 position,
            Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            foreach (char c in text)
            {
                b.DrawString(font, c + "",
                    position + new Vector2((widestCharacter - font.MeasureString(c + "").X) / 2, 0),
                    color, rotation, origin, scale, effects, layerDepth);

                position.X += widestCharacter;
            }
        }

        public static void Init(LocalizedContentManager.LanguageCode language)
        {
            SpriteFont font = ChatBox.messageFont(language);
            //This is theoretically right, but in practice makes everything too wide.
            /*widestCharacter = -1;
            for (int i = 32; i < 127; i++)
            {
                float width = font.MeasureString(char.ConvertFromUtf32(i)).X;
                if (width > widestCharacter)
                    widestCharacter = width;
            }*/

            widestCharacter = font.MeasureString("e").X;
        }

        public static float MeasureStringWidth(SpriteFont font, string text, bool isConsole)
        {
            if (!isConsole)
            {
                return font.MeasureString(text).X;
            }

            float widest = 0;
            foreach (string part in text.Split(new[] {"\n", "\r\n"}, StringSplitOptions.None))
                if (part.Length * widestCharacter > widest)
                    widest = part.Length * widestCharacter;
            return widest;
        }
    }
}