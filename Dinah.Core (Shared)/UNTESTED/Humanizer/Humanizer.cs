using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dinah.Core.Humanizer
{
    public class Humanizer
    {
        private DateTime last = DateTime.MinValue;

        /// <summary>Minimum wait time in seconds</summary>
        public int Minimum { get; set; } = 1;

        /// <summary>Maximum wait time in seconds</summary>
        public int Maximum { get; set; } = 3;

        /// <summary>The amount to alter a "second" by. Eg: Wobble=50 => 1 "second" will be b/t 950-1050 ms</summary>
        public int Wobble { get; set; } = 50;

        private static Random rand { get; } = new Random();
        private int waitTimeInMs()
        {
            var secondsWait = rand.Next(Minimum, Maximum);

            var secMin = 1000 - Wobble;
            var secMax = 1000 + Wobble;
            var randSecond = rand.Next(secMin, secMax);

            return secondsWait * randSecond;
        }

        public async Task Wait()
        {
            var bufferMilliseconds = waitTimeInMs();
            var elapsedMs = (DateTime.Now - last).TotalMilliseconds;

            if (bufferMilliseconds > elapsedMs)
            {
                var waitMs = bufferMilliseconds - (int)elapsedMs;
                await Task.Delay(waitMs);
            }

            last = DateTime.Now;
        }
    }
}
