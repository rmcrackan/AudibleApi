using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApiDTOs;
using Dinah.Core;
using Dinah.Core.Logging;
using Dinah.Core.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public enum DownloadQuality
    {
		Extreme,
		High,
		Normal,
		Low
	}
    public partial class Api
    {
        const string CONTENT_PATH = "/1.0/content";

		#region Download License

		/// <summary>
		/// Requests a license to download <see cref="DownloadQuality.Extreme"/> Audible content.
		/// </summary>
		/// <param name="asin">Audible Asin of book</param>
		/// <returns>Return a valid <see cref="ContentLicense"/> if successful; null if denied.</returns>
		public async Task<ContentLicense> GetDownloadLicenseAsync(string asin)
		{
			return await GetDownloadLicenseAsync(asin, DownloadQuality.Extreme);
		}
		/// <summary>
		/// Requests a license to download Audible content.
		/// </summary>
		/// <param name="asin">Audible Asin of book</param>
		/// <param name="quality">Desired audio Quality</param>
		/// <returns>Return a valid <see cref="ContentLicense"/> if successful; null if denied.</returns>
		public async Task<ContentLicense> GetDownloadLicenseAsync(string asin, DownloadQuality quality)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			var body = new JObject
			{
				{ "consumption_type", "Download" },
				{ "drm_type", "Adrm" },
				{ "quality", quality.ToString() },
				{ "response_groups", "last_position_heard,pdf_url,content_reference,chapter_info"}
			};
			var url = $"{CONTENT_PATH}/{asin}/licenserequest";

			var request = new HttpRequestMessage(HttpMethod.Post, url);
			request.AddContent(body);
			await signRequestAsync(request);

			//The caller logs exceptions thrown by this method on the Error level, so there's
			//no need to log errors here. However, the contents of error messages could be
			//useful in debugging the Api, so they are logged here in verbose only (because
			//they may contain customerId).

			HttpResponseMessage response;

			try
			{
				response = await _client.SendAsync(request);
			}
			catch (ApiErrorException ex)
			{
				Serilog.Log.Logger.Verbose(ex, $"Error requesting license for: {asin}  {{@DebugInfo}}", ex.JsonMessage.ToString(Formatting.None));
				throw;
			}

			if (response.StatusCode != HttpStatusCode.OK)
            {
				var ex = new ApiErrorException(response.Headers.Location, new JObject { { "http_response_code", response.StatusCode.ToString() }, { "response", await response.Content.ReadAsStringAsync() } }, "License response not OK");
				Serilog.Log.Logger.Verbose(ex, "License response not OK {@DebugInfo}.", ex.JsonMessage.ToString(Formatting.None));
				throw ex;
			}

			JObject responseJobj = await response.Content.ReadAsJObjectAsync();

			// if we get "message" on this level means something went wrong.
			// "message" should be nested under "content_license"
			if (responseJobj.TryGetValue("message", out var val))
			{
				var responseMessage = val.Value<string>();
				var ex = new ApiErrorException(response.Headers.Location, new JObject { { "error", responseMessage } });
				Serilog.Log.Logger.Verbose(ex, "License response returned error {@DebugInfo}", ex.JsonMessage.ToString(Formatting.None));
				throw ex;
			}

			ContentLicenseDtoV10 contentLicenseDtoV10;
			try
			{
				contentLicenseDtoV10 = ContentLicenseDtoV10.FromJson(responseJobj, _identityMaintainer.DeviceType, _identityMaintainer.DeviceSerialNumber, _identityMaintainer.AmazonAccountId);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"Error retrieving license for asin: {asin}");
				throw;
			}

			if (contentLicenseDtoV10?.ContentLicense?.StatusCode is null)
			{
				var ex = new ApiErrorException(response.Headers.Location, responseJobj,  "License response does not contain a valid status code.");
				Serilog.Log.Logger.Verbose(ex, "No status code {@DebugInfo}", ex.JsonMessage.ToString(Formatting.None));
				throw ex;
			}

			if (contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Denied"))
            {
				var ex = new ValidationErrorException(response.Headers.Location, responseJobj?["content_license"]?["license_denial_reasons"]?.Value<JObject>());
				Serilog.Log.Logger.Verbose(ex, "Content License denied {@DebugInfo}", ex.JsonMessage.ToString(Formatting.None));
				throw ex;
			}

			if (!contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Granted"))
			{
				var ex = new ApiErrorException(response.Headers.Location, new JObject { { "error", "Unexpected status_code: " + contentLicenseDtoV10.ContentLicense.StatusCode } });
				Serilog.Log.Logger.Verbose(ex, "Unrecognized status code {@DebugInfo}", ex.JsonMessage.ToString(Formatting.None));
				throw ex;
            }

			return contentLicenseDtoV10.ContentLicense;
		}

        #endregion

		public async Task<string> GetPdfDownloadLinkAsync(string asin)
		{
			//// SEPT 2020:

			//// no longer works. This url now returns 403
			// var downloadUrl = libraryBook?.Book?.Supplements?.FirstOrDefault()?.Url;

			//// this works for now:
			// MUST use relative url here
			var request = new HttpRequestMessage(HttpMethod.Head, $"/companion-file/{asin}");
			var client = Sharer.GetSharedHttpClient($"https://www.audible.{_locale.TopDomain}");
			var response = await AdHocAuthenticatedGetAsync(request, client);

			var downloadUrl = response.Headers.Location.AbsoluteUri;
			return downloadUrl;
		}
	}
}
