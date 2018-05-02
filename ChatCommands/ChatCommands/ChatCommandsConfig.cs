namespace ChatCommands
{
    class ChatCommandsConfig
    {
        public bool ListenToConsoleOnStartup { get; set; } = false;
        public int MaximumNumberOfHistoryMessages { get; set; } = 30;
    }
}
