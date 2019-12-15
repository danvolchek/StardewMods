namespace ModUpdateMenu.Updates
{
    /// <summary>An update status</summary>
    public enum UpdateStatus
    {
        /// <summary>Update checking was skipped.</summary>
        Skipped,

        /// <summary>The mod version is equal to remote version.</summary>
        UpToDate,

        /// <summary>The mod version is less than the remote version.</summary>
        OutOfDate,

        /// <summary>There was an error getting the remote version.</summary>
        Error
    }
}
