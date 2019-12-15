using StardewModdingAPI;
using System.Collections.Generic;

namespace ModUpdateMenu.Updates
{
    /// <summary>Can be notified about SMAPI/mod statuses.</summary>
    internal interface INotifiable
    {
        /*********
        ** Methods
        *********/
        /// <summary>Notify about mod statuses.</summary>
        /// <param name="statuses">The mod status.</param>
        void Notify(IList<ModStatus> statuses);

        /// <summary>Notifies about the SMAPI update version.</summary>
        /// <param name="version">The SMAPI update version.</param>
        void NotifySMAPI(ISemanticVersion version);
    }
}
