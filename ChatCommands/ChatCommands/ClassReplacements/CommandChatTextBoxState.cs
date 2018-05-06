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

        public CommandChatTextBoxState(int currentInsertPosition, int currentSnippetIndex, IEnumerable<ChatSnippet> finalText)
        {
            this.CurrentInsertPosition = currentInsertPosition;
            this.CurrentSnippetIndex = currentSnippetIndex;
            foreach (ChatSnippet snippet in finalText)
            {
                this.FinalText.Add(Utils.CopyChatSnippet(snippet));
            }
        }


    }
}
