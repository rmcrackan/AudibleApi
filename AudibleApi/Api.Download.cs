using AudibleApiDTOs;
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
        /// Requests a license to download Audible content.
        /// </summary>
        /// <param name="asin">Audible Asin of book</param>
        /// <param name="quality">Desired audio Quality</param>
        /// <returns>a valid <see cref="ContentLicense"/> containing content_reference, chapter_info, and pdf_url.</returns>
        /// <exception cref="ApiErrorException">Thrown when the Api request failed.</exception>
        /// <exception cref="InvalidResponseException">Thrown when the Api did not return a proper <see cref="ContentLicense"/>.</exception>
        /// <exception cref="InvalidValueException">Thrown when <see cref="ContentLicense.StatusCode"/> is not "Granted" or "Denied".</exception>
        /// <exception cref="ValidationErrorException">Thrown when <see cref="ContentLicense.StatusCode"/> is "Denied".</exception>
        public async Task<ContentLicense> GetDownloadLicenseAsync(string asin, DownloadQuality quality = DownloadQuality.Extreme)
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

            HttpResponseMessage response;
            try
            {
                response = await _client.SendAsync(request);
            }
            catch (ApiErrorException ex)
            {
                //Assume this exception will not contain PII.
                ex.LogException(Serilog.Log.Logger.Error);
                throw;
            }
            catch (Exception ex)
            {
                var apiExp = new ApiErrorException(
                    request.RequestUri,
                    body,
                    $"Error requesting license for asin: [{asin}]",
                    ex);

                apiExp.LogException(Serilog.Log.Logger.Error);
                throw apiExp;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var ex = new ApiErrorException(
                    response.Headers.Location,
                    //Assume this response does not contain PII.
                    new JObject { { "http_response_code", response.StatusCode.ToString() }, { "response", await response.Content.ReadAsStringAsync() } },
                    $"License response not \"OK\" for asin: [{asin}]");

                ex.LogException(Serilog.Log.Logger.Error);
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
                    response.Headers.Location,
                    responseJobj, //Even if the object doesn't parse, it may contain PII.
                    $"License response could not be parsed for asin: [{asin}]",
                    ex);

                apiExp.LogException(Serilog.Log.Logger.Verbose);
                throw apiExp;
            }

            if (contentLicenseDtoV10?.Message is not null)
            {
                var ex = new InvalidResponseException(
                    response.Headers.Location,
                    new JObject { { "message", contentLicenseDtoV10.Message } }, //Assume this message does not contain PII.
                    $"License response returned error for asin: [{asin}]");

                ex.LogException(Serilog.Log.Logger.Error);
                throw ex;
            }

            if (contentLicenseDtoV10?.ContentLicense?.StatusCode is null)
            {
                var ex = new InvalidValueException(
                    response.Headers.Location,
                    responseJobj, //This error shouldn't happen, so log the entire response which contains PII.
                    $"License response does not contain a valid status code for asin: [{asin}]");

                ex.LogException(Serilog.Log.Logger.Verbose);
                throw ex;
            }

            if (contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Denied"))
            {
                var ex = new ValidationErrorException(
                    response.Headers.Location,
                    //Denial reasons may contain PII.
                    new JObject { { "license_denial_reasons", JArray.FromObject(contentLicenseDtoV10.ContentLicense.LicenseDenialReasons) } },
                    $"Content License denied for asin: [{asin}]");

                ex.LogException(Serilog.Log.Logger.Verbose);
                throw ex;
            }

            if (!contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Granted"))
            {
                var ex = new InvalidValueException(
                    response.Headers.Location,
                    responseJobj, //This error shouldn't happen, so log the entire response which contains PII.
                    $"Unrecognized status code \"{contentLicenseDtoV10.ContentLicense.StatusCode}\" for asin: [{asin}]");

                ex.LogException(Serilog.Log.Logger.Verbose);
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
