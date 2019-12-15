using ChatCommands.Util;
using StardewValley.Menus;
using System.Collections.Generic;

namespace ChatCommands.ClassReplacements
{
    internal class CommandChatTextBoxState
    {
        internal readonly int CurrentInsertPosition;
        internal readonly long CurrentRecipientId;
        internal readonly string CurrentRecipientName;
        internal readonly int CurrentSnippetIndex;
        internal readonly List<ChatSnippet> FinalText = new List<ChatSnippet>();

        public CommandChatTextBoxState(int currentInsertPosition, int currentSnippetIndex, long currentRecipientId,
            string currentRecipientName, IEnumerable<ChatSnippet> finalText)
        {
            this.CurrentInsertPosition = currentInsertPosition;
            this.CurrentSnippetIndex = currentSnippetIndex;
            this.CurrentRecipientId = currentRecipientId;
            this.CurrentRecipientName = currentRecipientName;
            foreach (ChatSnippet snippet in finalText) this.FinalText.Add(Utils.CopyChatSnippet(snippet));
        }
    }
}
