namespace ModUpdateMenu.Updates
{
    public class ModStatus
    {
        public UpdateStatus UpdateStatus { get; }
        public string NewVersion { get; }
        public string CurrentVersion { get; }
        public string UpdateURL { get; }
        public string UpdateURLType { get; }
        public string ModName { get; }
        public string ModAuthor { get; }
        public string ErrorReason { get; }
        public string Id { get; }

        private ModStatus(UpdateStatus updateStatus, string Id, string name, string author, string updateURL)
        {
            this.UpdateStatus = updateStatus;
            this.Id = Id;
            this.ModName = name;
            this.ModAuthor = author;
            this.UpdateURL = updateURL;
            if (updateURL.ToLower().Contains("nexusmods"))
                this.UpdateURLType = "NexusMods";
            else if (updateURL.ToLower().Contains("github"))
                this.UpdateURLType = "Github";
            else if (updateURL.ToLower().Contains("playstarbound"))
                this.UpdateURLType = "Forums";
            else if (updateURL.ToLower().Contains("stardewvalleywiki"))
                this.UpdateURLType = "Unofficial";
            else this.UpdateURLType = "???";
        }

        public ModStatus(UpdateStatus updateStatus, string Id, string name, string author, string updateURL, string currentVersion, string newVersion = null, string errorReason = null) : this(updateStatus, Id, name, author, updateURL)
        {
            this.NewVersion = newVersion;
            this.CurrentVersion = currentVersion;
            this.NewVersion = newVersion;
            this.ErrorReason = errorReason;
        }
    }
}