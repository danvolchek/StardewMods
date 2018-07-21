using System.Collections.Generic;

namespace ModUpdateMenu.Updates
{
    internal interface IUpdateStatusRetriever
    {
        bool GetUpdateStatuses(out IList<ModStatus> statuses);
    }
}
