using System;
using System.IO;
using System.Text;

namespace Dinah.Core.IO
{
    // from: https://stackoverflow.com/a/18727100

    public class FileLoggerTextWriter : TextWriter
    {
        FileLogger logger1 { get; }

        public FileLoggerTextWriter(FileLogger logger1) => this.logger1 = logger1;

        public override void WriteLine(string value) => logger1.TextWriterLogger(value);
        public override void Write(char value) => logger1.TextWriterLogger(value.ToString());
        public override void Write(string value) => logger1.TextWriterLogger(value);
        public override Encoding Encoding => Encoding.ASCII;
    }
}
