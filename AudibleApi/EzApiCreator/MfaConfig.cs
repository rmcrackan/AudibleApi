using System;

namespace AudibleApi
{
	public class MfaConfig
	{
		// optional string settings
		public string Title { get; set; }
		public string Button1Text { get; set; }
		public string Button2Text { get; set; }
		public string Button3Text { get; set; }

		// mandatory values
		public string Button1Name { get; set; }
		public string Button1Value { get; set; }

		public string Button2Name { get; set; }
		public string Button2Value { get; set; }

		public string Button3Name { get; set; }
		public string Button3Value { get; set; }
	}
}
