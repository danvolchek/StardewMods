using StardewModdingAPI;

namespace ModUpdateMenu.Updates
{
    /// <summary>A mod status.</summary>
    internal class ModStatus
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The update status.</summary>
        public UpdateStatus UpdateStatus { get; }

        /// <summary>The new version.</summary>
        public string NewVersion { get; }

        /// <summary>The current version.</summary>
        public string CurrentVersion => this.manifest.Version.ToString();

        /// <summary>The update url.</summary>
        public string UpdateURL { get; }

        /// <summary>The update url type.</summary>
        public string UpdateURLType { get; }

        /// <summary>The mod name.</summary>
        public string ModName => this.manifest.Name;

        /// <summary>The mod author.</summary>
        public string ModAuthor => this.manifest.Author;

        /// <summary>The reason an error occured while checking for updates.</summary>
        public string ErrorReason { get; }

        /// <summary>The mod unique id.</summary>
        public string Id => this.manifest.UniqueID;

        
        /*********
        ** Fields
        *********/
        /// <summary>The mod manifest.</summary>
        private readonly IManifest manifest;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="updateStatus">The update status.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="updateURL">The update url.</param>
        /// <param name="newVersion">The new version.</param>
        /// <param name="errorReason">The reason an error occured while checking for updates.</param>
        public ModStatus(UpdateStatus updateStatus, IManifest manifest, string updateURL, string newVersion = null, string errorReason = null)
        {
            this.manifest = manifest;
            this.NewVersion = newVersion;
            this.ErrorReason = errorReason;
            this.UpdateStatus = updateStatus;
            this.UpdateURL = updateURL;
            if (updateURL.ToLower().Contains("nexusmods"))
                this.UpdateURLType = "NexusMods";
            else if (updateURL.ToLower().Contains("github"))
                this.UpdateURLType = "Github";
            else if (updateURL.ToLower().Contains("playstarbound"))
                this.UpdateURLType = "Forums";
            else if (updateURL.ToLower().Contains("stardewvalleywiki"))
                this.UpdateURLType = "Unofficial";
            else if (updateURL.ToLower().Contains("spacechase0"))
                this.UpdateURLType = "Spacechase";
            else this.UpdateURLType = "???";
        }
    }
}
