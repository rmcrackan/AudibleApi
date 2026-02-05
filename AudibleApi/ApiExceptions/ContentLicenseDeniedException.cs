using AudibleApi.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AudibleApi;

public class ContentLicenseDeniedException : AudibleApiException
{
	private static readonly Regex CustomerIdPattern = new("\\[A\\w{10,20}\\]", RegexOptions.Compiled);
	public LicenseDenialReason? Client { get; }
	public LicenseDenialReason? Ownership { get; }
	public LicenseDenialReason? Membership { get; }
	public LicenseDenialReason? AYCL { get; }

	public ContentLicenseDeniedException(Uri? requestUri, ContentLicense? license) : this(requestUri, license, null) { }

	public ContentLicenseDeniedException(Uri? requestUri, ContentLicense? license, Exception? innerException) : base(requestUri, null, $"Content License denied for asin: [{license?.Asin ?? "[null]"}]", innerException)
	{
		if (license?.LicenseDenialReasons is null)
		{
			JsonMessage = new JObject { { "license_denial_reasons", "NO REASONS GIVEN" } }.ToString(Newtonsoft.Json.Formatting.None);
			return;
		}

		var reasonList = license.LicenseDenialReasons.Select(r =>
		{
			if (r.Message is not null)
				r.Message = CustomerIdPattern.Replace(r.Message, "[##############]"); //Replace personally identifying customer ID.
			return JObject.FromObject(r);
		});

		JsonMessage = new JObject { { "license_denial_reasons", JArray.FromObject(reasonList) } }.ToString(Newtonsoft.Json.Formatting.None);

		// This should properly be Single() not FirstOrDefault(), but FirstOrDefault is defensive for malformed data from audible
		Client = license.LicenseDenialReasons?.FirstOrDefault(r => r.ValidationType == ValidationType.Client);
		Ownership = license.LicenseDenialReasons?.FirstOrDefault(r => r.ValidationType == ValidationType.Ownership);
		Membership = license.LicenseDenialReasons?.FirstOrDefault(r => r.ValidationType == ValidationType.Membership);
		AYCL = license.LicenseDenialReasons?.FirstOrDefault(r => r.ValidationType == ValidationType.AYCL);
	}
}
