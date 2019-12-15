using System;
using System.Diagnostics;

namespace CopyInviteCode.ClipboardManagers
{
    /// <summary>Sets clipboard contents on Unix.</summary>
    internal abstract class UnixClipboardManager : IClipboardManager
    {
        protected abstract string FileName { get; }
        protected abstract string SetArguments { get; }

        /// <summary>
        ///     Sets clipboard text on a unix like system by directly invoking a native process.
        ///     Taken from https://stackoverflow.com/questions/28611112/mono-clipboard-fix/33563898#33563898.
        /// </summary>
        public void SetText(string textToCopy)
        {
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo =
                        new ProcessStartInfo(this.FileName, this.SetArguments)
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = false,
                            RedirectStandardInput = true
                        };
                    p.Start();
                    p.StandardInput.Write(textToCopy);
                    p.StandardInput.Close();
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
