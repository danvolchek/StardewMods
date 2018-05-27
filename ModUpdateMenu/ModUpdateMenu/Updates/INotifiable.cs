using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUpdateMenu.Updates
{
    interface INotifiable
    {
        void Notify(IList<ModStatus> statuses);
    }
}
