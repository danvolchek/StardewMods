using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace ChatCommands.ClassReplacements
{
    /// <summary>
    /// We need to overwrite <see cref="EmojiMenu.leftClick"/> to correctly call
    /// <see cref="CommandChatTextBox.ReceiveEmoji"/> since the latter is not virtual.
    /// </summary>
    internal class CommandEmojiMenu : EmojiMenu
    {
        private readonly ClickableComponent bUpArrow;
        private readonly ClickableComponent bDownArrow;
        private readonly ClickableComponent bSendArrow;
        private readonly List<ClickableComponent> bEmojiSelectionButtons;
        private readonly IReflectedField<int> bPageStartIndex;
        private readonly IReflectedMethod bUpArrowPressed;
        private readonly IReflectedMethod bDownArrowPressed;

        public CommandEmojiMenu(IReflectionHelper reflection, ChatBox chatBox, Texture2D emojiTexture, Texture2D chatBoxTexture) : base(chatBox, emojiTexture, chatBoxTexture)
        {
            this.bUpArrow = reflection.GetField<ClickableComponent>(this, "upArrow").GetValue();
            this.bDownArrow = reflection.GetField<ClickableComponent>(this, "downArrow").GetValue();
            this.bSendArrow = reflection.GetField<ClickableComponent>(this, "sendArrow").GetValue();
            this.bEmojiSelectionButtons = reflection.GetField<List<ClickableComponent>>(this, "emojiSelectionButtons").GetValue();
            this.bPageStartIndex = reflection.GetField<int>(this, "pageStartIndex");
            this.bUpArrowPressed = reflection.GetMethod(this, "upArrowPressed");
            this.bDownArrowPressed = reflection.GetMethod(this, "downArrowPressed");
        }

        /// <summary>
        /// Handle a left click properly.
        /// </summary>
        public void LeftClick(int x, int y, ChatBox cb)
        {
            if (!this.isWithinBounds(x, y))
                return;
            int x1 = x - this.xPositionOnScreen;
            int y1 = y - this.yPositionOnScreen;
            if (this.bUpArrow.containsPoint(x1, y1))
                this.bUpArrowPressed.Invoke(30);
            else if (this.bDownArrow.containsPoint(x1, y1))
                this.bDownArrowPressed.Invoke(30);
            else if (this.bSendArrow.containsPoint(x1, y1) && cb.chatBox.currentWidth > 0.0)
            {
                (cb as CommandChatBox)?.EnterPressed(cb.chatBox);
                this.bSendArrow.scale = 0.5f;
                Game1.playSound("shwip");
            }
            foreach (ClickableComponent emojiSelectionButton in this.bEmojiSelectionButtons)
            {
                if (!emojiSelectionButton.containsPoint(x1, y1)) continue;
                int emoji = this.bPageStartIndex.GetValue() + int.Parse(emojiSelectionButton.name);
                (cb.chatBox as CommandChatTextBox)?.ReceiveEmoji(emoji);
                Game1.playSound("coin");
                break;
            }
        }
    }
}
