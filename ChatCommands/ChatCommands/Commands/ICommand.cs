using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace ChatCommands.Commands
{
    internal interface ICommand
    {
        void Register(ICommandHelper helper);
    }
}
