using System.Collections.Generic;

namespace ChatCommands
{
    /// <summary>Mod configuration.</summary>
    internal class ChatCommandsConfig
    {
        public bool ListenToConsoleOnStartup { get; set; } = false;
        public int MaximumNumberOfHistoryMessages { get; set; } = 70;
        public bool UseMonospacedFontForCommandOutput { get; set; } = true;
        public bool RemoveSMAPIMessagePrefix { get; set; } = false;
        public IDictionary<string, string> ColorOverrides { get; set; } = new Dictionary<string, string>();
    }
}
