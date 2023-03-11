using AudibleApi.Common;
using Dinah.Core;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudibleApi
{
    public enum DownloadQuality
    {
        High,
        Normal
    }

    public enum ChapterTitlesType
    {
        Flat,
        Tree
    }

	public partial class Api
    {
		#region Download License

		/// <summary>
		/// Requests a license to download Audible content.
		/// </summary>
		/// <param name="asin">Audible Asin of book</param>
		/// <param name="quality">Desired audio Quality</param>
		/// <returns>a valid <see cref="ContentLicense"/> containing content_reference, chapter_info, and pdf_url.</returns>
		/// <exception cref="ApiErrorException">Thrown when the Api request failed.</exception>
		/// <exception cref="InvalidResponseException">Thrown when the Api did not return a proper <see cref="ContentLicense"/>.</exception>
		/// <exception cref="InvalidValueException">Thrown when <see cref="ContentLicense.StatusCode"/> is not "Granted" or "Denied".</exception>
		/// <exception cref="ContentLicenseDeniedException">Thrown when <see cref="ContentLicense.StatusCode"/> is "Denied".</exception>
		public async Task<ContentLicense> GetDownloadLicenseAsync(string asin, DownloadQuality quality = DownloadQuality.High, ChapterTitlesType chapterTitlesType = ChapterTitlesType.Tree)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

            var body = new JObject
            {
                { "consumption_type", "Download" },
                { "supported_drm_types", new JArray{ "Adrm", "Mpeg" } },
                { "quality", quality.ToString() },
                { "chapter_titles_type", chapterTitlesType.ToString() },
                { "response_groups", "last_position_heard,pdf_url,content_reference,chapter_info"}
            };

            string requestUri = $"{CONTENT_PATH}/{asin}/licenserequest";
            HttpResponseMessage response;
            try
            {
                response = await AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Post, Client, body);
            }
            catch (ApiErrorException ex)
            {
                //Assume this exception will not contain PII.
                Serilog.Log.Logger.Error(ex, "Error getting download license");
                throw;
            }
            catch (Exception ex)
            {
                var apiExp = new ApiErrorException(
                    requestUri,
                    body,
                    $"Error requesting license for asin: [{asin}]",
                    ex);
                Serilog.Log.Logger.Error(apiExp, "Error requesting download license");
                throw apiExp;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var ex = new ApiErrorException(
                    response.RequestMessage.RequestUri,
                    //Assume this response does not contain PII.
                    new JObject { { "http_response_code", response.StatusCode.ToString() }, { "response", await response.Content.ReadAsStringAsync() } },
                    $"License response not \"OK\" for asin: [{asin}]");
                Serilog.Log.Logger.Error(ex, "Download response does not contain a valid status code");
                throw ex;
            }

            var responseJobj = await response.Content.ReadAsJObjectAsync();

            ContentLicenseDtoV10 contentLicenseDtoV10;
            try
            {
                contentLicenseDtoV10 = ContentLicenseDtoV10.FromJson(responseJobj, _identityMaintainer.DeviceType, _identityMaintainer.DeviceSerialNumber, _identityMaintainer.AmazonAccountId);
            }
            catch (Exception ex)
            {
                var apiExp = new InvalidResponseException(
                    response.RequestMessage.RequestUri,
                    responseJobj, //Even if the object doesn't parse, it may contain PII.
                    $"License response could not be parsed for asin: [{asin}]",
                    ex);
                Serilog.Log.Logger.Verbose(apiExp, "License response could not be parsed");
                throw apiExp;
            }

            if (contentLicenseDtoV10?.Message is not null)
            {
                var ex = new InvalidResponseException(
                    response.RequestMessage.RequestUri,
                    new JObject { { "message", contentLicenseDtoV10.Message } }, //Assume this message does not contain PII.
                    $"License response returned error for asin: [{asin}]");
                Serilog.Log.Logger.Error(ex, "License response returned error");
                throw ex;
            }

            if (contentLicenseDtoV10?.ContentLicense?.StatusCode is null)
            {
                var ex = new InvalidValueException(
                    response.RequestMessage.RequestUri,
                    responseJobj, //This error shouldn't happen, so log the entire response which contains PII.
                    $"License response does not contain a valid status code for asin: [{asin}]");
                Serilog.Log.Logger.Verbose(ex, "License response does not contain a valid status code");
                throw ex;
            }

            if (contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Denied"))
            {
                var ex = new ContentLicenseDeniedException(response.RequestMessage.RequestUri, contentLicenseDtoV10.ContentLicense);

                Serilog.Log.Logger.Error(ex, "Content License denied");
                throw ex;
            }

            if (!contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Granted"))
            {
                var ex = new InvalidValueException(
                    response.RequestMessage.RequestUri,
                    responseJobj, //This error shouldn't happen, so log the entire response which contains PII.
                    $"Unrecognized status code \"{contentLicenseDtoV10.ContentLicense.StatusCode}\" for asin: [{asin}]");
                Serilog.Log.Logger.Verbose(ex, "Unrecognized status code");
                throw ex;
            }

            return contentLicenseDtoV10.ContentLicense;
        }

        #endregion

        public async Task<string> GetPdfDownloadLinkAsync(string asin) => (await GetDownloadLicenseAsync(asin)).PdfUrl;
    }
}
