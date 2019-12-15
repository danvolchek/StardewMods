namespace CopyInviteCode.ClipboardManagers
{
    /// <summary>Sets clipboard contents on Linux.</summary>
    internal class LinuxClipboardManager : UnixClipboardManager
    {
        protected override string FileName => "xclip";
        protected override string SetArguments => "-selection clipboard";
    }
}
