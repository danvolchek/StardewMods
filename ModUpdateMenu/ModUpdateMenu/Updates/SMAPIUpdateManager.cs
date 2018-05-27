using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ModUpdateMenu.Extensions.ListExtensions;
namespace ModUpdateMenu.Updates
{
    internal class SMAPIUpdateManager
    {
        //TODO: get update info (console redirection? harmony transpile?)

        private INotifiable rootNotifiable;

        private NotifyingTextWriter consoleNotifier;

        private IList<Tuple<Regex, UpdateStatus>> updatePatters = new List<Tuple<Regex, UpdateStatus>>()
        {
            {new Regex("Couldn't check for a new version of SMAPI.", RegexOptions.Compiled), UpdateStatus.Error },
            {new Regex("You can update SMAPI to (.*): (.*)", RegexOptions.Compiled), UpdateStatus.OutOfDate },
            {new Regex("   SMAPI okay.", RegexOptions.Compiled), UpdateStatus.UpToDate }

        };

        public SMAPIUpdateManager(INotifiable notifiable)
        {
            this.rootNotifiable = notifiable;

            this.consoleNotifier = new NotifyingTextWriter(Console.Out, this.OnLineWritten);
            Console.SetOut(this.consoleNotifier);
            this.consoleNotifier.IsNotifying = true;
        }

        /// <summary>
        ///     When a line is written to the console, add it to the chatbox.
        /// </summary>
        private void OnLineWritten(char[] buffer, int index, int count)
        {
            string text = string.Join("", buffer.Skip(index).Take(count)).Trim();

            if (text.EndsWith("SMAPI] Checking for updates..."))
            {
                int x = 5;
            }
        }
    }
}
