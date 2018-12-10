using System.Collections.Generic;
using StardewModdingAPI;

namespace ModUpdateMenu.Updates
{
    internal interface IUpdateStatusRetriever
    {
        bool GetUpdateStatuses(out IList<ModStatus> statuses);
        ISemanticVersion GetSMAPIUpdateVersion();
    }
}
