using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Dinah.Core.IO
{
    public static class FileExt
    {
        public static void SafeDelete(string source)
        {
            while (true)
            {
                try
                {
                    // deletes file if it exists. no error if it doesn't exist
                    File.Delete(source);
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(100);
                    Console.WriteLine($"Failed to delete {source}. Exception: {e.Message}");
                }
            }
        }

        public static void SafeMove(string source, string target)
        {
            while (true)
            {
                try
                {
                    if (File.Exists(source))
                    {
                        File.Delete(target);
                        File.Move(source, target);
                    }

                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(100);
                    Console.WriteLine($"Failed to move {source} to {target}. Exception: {e.Message}");
                }
            }
        }

        public static void CreateFile(string file, byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            using (var fileStream = File.Create(file))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.CopyTo(fileStream);
            }
        }
    }
}
