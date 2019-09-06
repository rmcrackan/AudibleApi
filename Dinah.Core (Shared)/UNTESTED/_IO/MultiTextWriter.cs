using System;
using System.IO;
using System.Text;

namespace Dinah.Core.IO
{
    // from: https://stackoverflow.com/a/18727100
    // use example
    // Console.SetOut(new MultiTextWriter(new ControlWriter(textbox1), Console.Out));
    public class MultiTextWriter : TextWriter
    {
        private TextWriter[] writers { get; }
        //public MultiTextWriter(IEnumerable<TextWriter> writers) => this.writers = writers.ToList();
        public MultiTextWriter(params TextWriter[] writers) => this.writers = writers;

        public override void WriteLine(string value)
        {
            foreach (var writer in writers)
                writer.WriteLine(value);
        }

        public override void Write(char value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void Write(string value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void Flush()
        {
            foreach (var writer in writers)
                writer.Flush();
        }

        public override void Close()
        {
            foreach (var writer in writers)
                writer.Close();
        }

        public override Encoding Encoding => Encoding.ASCII;
    }
}
