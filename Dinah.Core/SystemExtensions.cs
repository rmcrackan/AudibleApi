using System;

namespace Dinah.Core
{
    public static class SystemExtensions
    {
        public static long ToUnixTimeStamp(this DateTime dateTime)
            => (long)(TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        public static string ToRfc3339String(this DateTime dateTime)
            => System.Xml.XmlConvert.ToString(dateTime, System.Xml.XmlDateTimeSerializationMode.Utc);

		public static string GetOrigin(this Uri uri)
			=> uri.GetLeftPart(UriPartial.Authority);
	}
}
