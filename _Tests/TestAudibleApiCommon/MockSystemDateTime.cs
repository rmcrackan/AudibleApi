using System;
using BaseLib;

namespace TestAudibleApiCommon
{
	public class StaticSystemDateTime : ISystemDateTime
	{
		public DateTime Now { get; set; }
		public DateTime UtcNow { get; set; }

		public static StaticSystemDateTime ByYear(int year)
		{
			var local = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Local);
			var utc = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			return new StaticSystemDateTime { Now = local, UtcNow = utc };
		}
	}
}
