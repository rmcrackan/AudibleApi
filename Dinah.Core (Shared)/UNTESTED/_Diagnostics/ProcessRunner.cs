using System.Diagnostics;

namespace Dinah.Core.Diagnostics
{
    public static class ProcessRunner
    {
        public static string WorkingDir { get; set; } = System.IO.Path.GetDirectoryName(Exe.FileLocationOnDisk);

        public static (string Output, int ExitCode) RunHidden(
            this ProcessStartInfo seedInfo,
            bool readErrorOutput = false)
        {
            string output;
            int exitCode;

            using (var process = new Process { StartInfo = seedInfo })
            {
                process.StartInfo.RedirectStandardOutput = !readErrorOutput;
                process.StartInfo.RedirectStandardError = readErrorOutput;

                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = false;

                if (string.IsNullOrWhiteSpace(process.StartInfo.WorkingDirectory))
                    process.StartInfo.WorkingDirectory = WorkingDir;

                process.Start();
                output = readErrorOutput
                    ? process.StandardError.ReadToEnd()
                    : process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                exitCode = process.ExitCode;

                process.Close();
            }

            return (output, exitCode);
        }
    }
}
