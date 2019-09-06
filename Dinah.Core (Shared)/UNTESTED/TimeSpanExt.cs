using System;

namespace Dinah.Core
{
    public static class TimeSpanExt
    {
        public static string GetTotalTimeFormatted(this TimeSpan timeSpan)
            => ((int)timeSpan.TotalHours).ToString("D2")
            + ":" + timeSpan.Minutes.ToString("D2")
            + ":" + timeSpan.Seconds.ToString("D2");
    }
}
