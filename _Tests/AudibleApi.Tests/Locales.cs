using AudibleApi;
using System;

namespace TestAudibleApiCommon
{
	public static class Locales
	{
		public static Locale Us => Localization.Get(UsName);
		public static Locale Uk => Localization.Get(UkName);
		public static Locale Germany => Localization.Get(GermanyName);
		public static Locale France => Localization.Get(FranceName);
		public static Locale Canada => Localization.Get(CanadaName);
		public static Locale Australia => Localization.Get(AustraliaName);

		public const string UsName = "us";
		public const string UkName = "uk";
		public const string GermanyName = "germany";
		public const string FranceName = "france";
		public const string CanadaName = "canada";
		public const string AustraliaName = "australia";
	}
}
