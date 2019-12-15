namespace ChatCommands
{
    /// <summary>Mod configuration.</summary>
    internal class ChatCommandsConfig
    {
        public bool ListenToConsoleOnStartup { get; set; } = false;
        public int MaximumNumberOfHistoryMessages { get; set; } = 70;
        public bool UseMonospacedFontForCommandOutput { get; set; } = true;
    }
}
