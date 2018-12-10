using StardewModdingAPI;
using System.Collections.Generic;

namespace ModUpdateMenu.Updates
{
    internal interface INotifiable
    {
        void Notify(IList<ModStatus> statuses);
        void NotifySMAPI(ISemanticVersion version);
    }
}
