using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using ChatCommands.Util;

namespace ChatCommands.ClassReplacements
{
    internal class CommandChatTextBoxState
    {
        internal readonly int CurrentInsertPosition;
        internal readonly int CurrentSnippetIndex;
        internal readonly List<ChatSnippet> FinalText = new List<ChatSnippet>();
        internal readonly long CurrentRecipientId;

        public CommandChatTextBoxState(int currentInsertPosition, int currentSnippetIndex, long currentRecipientId, IEnumerable<ChatSnippet> finalText)
        {
            this.CurrentInsertPosition = currentInsertPosition;
            this.CurrentSnippetIndex = currentSnippetIndex;
            this.CurrentRecipientId = currentRecipientId;
            foreach (ChatSnippet snippet in finalText)
            {
                this.FinalText.Add(Utils.CopyChatSnippet(snippet));
            }
        }


    }
}
