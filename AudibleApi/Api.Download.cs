using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApiDTOs;
using Dinah.Core;
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

			var response = await _client.SendAsync(request);
			var responseJobj = await response.Content.ReadAsJObjectAsync();

			// if we get "message" on this level means something went wrong.
			// "message" should be nested under "content_license"
			if (responseJobj.TryGetValue("message", out var val))
			{
				var responseMessage = val.Value<string>();
				throw new ApiErrorException(response.Headers.Location, new JObject { { "error", responseMessage } });
			}

			ContentLicenseDtoV10 contentLicenseDtoV10;
			try
			{
				contentLicenseDtoV10 = ContentLicenseDtoV10.FromJson(responseJobj, _identityMaintainer.DeviceType, _identityMaintainer.DeviceSerialNumber, _identityMaintainer.AmazonAccountId);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"Error retrieving content metadata for asin: {asin}");
				throw;
			}

			if (contentLicenseDtoV10?.ContentLicense?.StatusCode is null)
			{
				var ex = new ApiErrorException(response.Headers.Location, responseJobj,  "License response does not contain a valid status code.");
				Serilog.Log.Logger.Error(ex, "No status code {@DebugInfo}");
				throw ex;
			}

			if (contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Denied"))
            {
				var ex = new ValidationErrorException(response.Headers.Location, responseJobj?["content_license"]?["license_denial_reasons"]?.Value<JObject>());
				Serilog.Log.Logger.Error(ex, "Content License denied {@DebugInfo}");
				throw ex;
			}

			if (!contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Granted"))
			{
				var ex = new ApiErrorException(response.Headers.Location, new JObject { { "error", "Unexpected status_code: " + contentLicenseDtoV10.ContentLicense.StatusCode } });
				Serilog.Log.Logger.Error(ex, "Unrecognized status code {@DebugInfo}");
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
