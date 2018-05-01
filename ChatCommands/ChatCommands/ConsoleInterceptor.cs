using System.IO;
using System.Text;

namespace ChatCommands
{
    /// <summary>
    /// A <see cref="TextWriter"/> that intercepts another <see cref="TextWriter"/> and notifes anything written to it.
    /// </summary>
    /// <remarks>
    /// Borrowed heavily from https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/Framework/Logging/InterceptingTextWriter.cs
    /// </remarks>
    class NotifyingTextWriter : TextWriter
    {
        private TextWriter original;
        private OnLineWritten callback;
        public bool isRedirecting;

        public override Encoding Encoding => this.original.Encoding;

        public NotifyingTextWriter(TextWriter original, OnLineWritten callback)
        {
            this.original = original;
            this.callback = callback;
            this.isRedirecting = false;
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if(this.isRedirecting)
                this.callback(buffer, index, count);
            this.original.Write(buffer, index, count);
        }

        public override void Write(char ch)
        {
            this.original.Write(ch);
        }

        public delegate void OnLineWritten(char[] buffer, int index, int count);
    }
}
