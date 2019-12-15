using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ColorfulConsole
{
    internal class ColorfulTextWriter : TextWriter
    {
        private readonly ConsoleColor[] colors = ((ConsoleColor[])Enum.GetValues(typeof(ConsoleColor))).Where(color => color != ConsoleColor.Black).ToArray();
        private readonly Random rand = new Random();

        private readonly TextWriter original;

        public ColorfulTextWriter(TextWriter original)
        {
            this.original = original;
        }

        public override Encoding Encoding => this.original.Encoding;

        public override void Write(char[] buffer, int index, int count)
        {
            this.SetRandomColor();
            this.original.Write(buffer, index, count);
        }

        public override void Write(char ch)
        {
            this.SetRandomColor();
            this.original.Write(ch);
        }

        private void SetRandomColor()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = this.GetRandomColor();
        }

        private ConsoleColor GetRandomColor()
        {
            return this.colors[this.rand.Next(0, this.colors.Length)];
        }

    }
}
