using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dinah.Core;

namespace Dinah.Core.StepRunner
{
    public abstract class BaseStep
    {
        public string Name { get; set; }

        protected abstract bool RunRaw();

        public (bool IsSuccess, TimeSpan Elapsed) Run()
        {
            Console.WriteLine($"Begin step '{Name}'");
            var stopwatch = Stopwatch.StartNew();

            bool success;
            try
            {
                success = RunRaw();
            }
            catch
            {
                success = false;
            }

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;

            Console.WriteLine(
                $"End step '{Name}'. "
                + (success ? "Success" : "FAILED")
                + ". Completed in " + elapsed.GetTotalTimeFormatted());

            return (success, elapsed);
        }
    }
}
