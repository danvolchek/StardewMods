using System.Windows.Forms;

namespace CopyInviteCode.ClipboardManagers
{
    /// <summary>Sets clipboard contents on Windows.</summary>
    internal class WindowsClipboardManager : IClipboardManager
    {
        public void SetText(string text)
        {
            Clipboard.SetText(text);
        }
    }
}
