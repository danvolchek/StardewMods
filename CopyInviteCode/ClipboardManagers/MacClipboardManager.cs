namespace CopyInviteCode.ClipboardManagers
{
    /// <summary>Sets clipboard contents on Mac.</summary>
    internal class MacClipboardManager : UnixClipboardManager
    {
        protected override string FileName => "pbcopy";
        protected override string SetArguments => "-pboard general -Prefer txt";
    }
}
