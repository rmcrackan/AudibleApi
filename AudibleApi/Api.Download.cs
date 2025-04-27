using AudibleApi.Common;
using Dinah.Core;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        #region DrmLicense

        public async Task<string> WidevineDrmLicense(string asin, string licenseChallenge)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));
            string requestUri = $"{CONTENT_PATH}/{asin}/drmlicense";
            var body = new JObject
            {
                { "consumption_type",  "Download" },
                { "drm_type", "Widevine" },
                { "tenant_id", "Audible" },
                { "licenseChallenge", licenseChallenge }
            };
            HttpResponseMessage response;
            try
            {
                response = await AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Post, Client, body);
            }
            catch (ApiErrorException ex)
            {
                Serilog.Log.Logger.Error(ex, "Error getting DRM license");
                throw;
            }
            catch (Exception ex)
            {
                var apiExp = new ApiErrorException(
                    requestUri,
                    body,
                    $"Error requesting DRM license for asin: [{asin}]",
                    ex);
                Serilog.Log.Logger.Error(apiExp, "Error requesting DRM license");
                throw apiExp;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var ex = new ApiErrorException(
                    response.RequestMessage.RequestUri,
                    //Assume this response does not contain PII.
                    new JObject { { "http_response_code", response.StatusCode.ToString() }, { "response", await response.Content.ReadAsStringAsync() } },
                    $"DRM license response not \"OK\" for asin: [{asin}]");
                Serilog.Log.Logger.Error(ex, "DRM license response does not contain a valid status code");
                throw ex;
            }

            var responseJobj = await response.Content.ReadAsJObjectAsync();
            DrmLicenseDtov10 drmLicenseDtoV10;
            try
            {
                drmLicenseDtoV10 = DrmLicenseDtov10.FromJson(responseJobj);
            }
            catch (Exception ex)
            {
                var apiExp = new InvalidResponseException(
                    response.RequestMessage.RequestUri,
                    responseJobj, //Even if the object doesn't parse, it may contain PII.
                    $"DRM license response could not be parsed for asin: [{asin}]",
                    ex);
                Serilog.Log.Logger.Verbose(apiExp, "DRM license response could not be parsed");
                throw apiExp;
            }

            if (drmLicenseDtoV10?.Message is not null)
            {
                var ex = new InvalidResponseException(
                    response.RequestMessage.RequestUri,
                    new JObject { { "message", drmLicenseDtoV10.Message }, { "reason", drmLicenseDtoV10.Reason } },
                    $"DRM license response returned error for asin: [{asin}]");
                Serilog.Log.Logger.Error(ex, "DRM license response returned error");
                throw ex;
            }

            if (drmLicenseDtoV10?.License is null)
            {
                var ex = new InvalidValueException(
                    response.RequestMessage.RequestUri,
                    responseJobj, //This error shouldn't happen, so log the entire response which contains PII.
                    $"DRM license response does not contain a valid license for asin: [{asin}]");
                Serilog.Log.Logger.Verbose(ex, "DRM license response does not contain a valid status code");
                throw ex;
            }

            return drmLicenseDtoV10.License;
        }

        #endregion

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
        public async Task<ContentLicense> GetDownloadLicenseAsync(string asin, DownloadQuality quality = DownloadQuality.High, ChapterTitlesType chapterTitlesType = ChapterTitlesType.Tree, DrmType drmType = DrmType.Adrm, bool spatial = false, params string[] additionalCodecs)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			//Widevine content will only be delivered to Android devices
			if (drmType == DrmType.Widevine && _identityMaintainer.DeviceType != Resources.DeviceType)
				drmType = DrmType.Adrm;

            //Always request AAC codecs
            Array.Resize(ref additionalCodecs, additionalCodecs.Length + 2);
            additionalCodecs[^2] = "mp4a.40.2"; //AAC-LC
            additionalCodecs[^1] = "mp4a.40.42"; //xHE-AAC

			var body = new JObject
            {
                { "supported_media_features", new JObject
                    {
                    { "drm_types", new JArray { drmType.ToString(), "Mpeg" } },
                    { "codecs", new JArray(additionalCodecs) },
                    { "chapter_titles_type", chapterTitlesType.ToString() },
                    { "previews", false },
                    { "catalog_samples", false }
                    }
                },
                { "spatial", spatial },
                { "consumption_type", "Download" },
                { "tenant_id", "Audible" },
                { "quality", quality.ToString() },
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
				Serilog.Log.Logger.Error(ex, "Error getting download license. Try aax workaround.");
				try
                {
                    //This book doesn't exist as an aaxc. Fall back to old AAX workaround method and try again.
                    var url = await DownloadAaxWorkaroundAsync(asin);
                    
                    var contentLic = new ContentLicense
                    {
                        DrmType = DrmType.Adrm,
                        Asin = asin,
                        ContentMetadata = await GetContentMetadataAsync(asin, chapterTitlesType),
                        Voucher = new VoucherDtoV10
                        {
                            Key = await GetActivationBytesAsync(),
                        }
                    };

					contentLic.ContentMetadata.ContentUrl = new ContentUrl { OfflineUrl = url };

					return contentLic;
                }
                catch (Exception ex2)
                {
					//Assume this exception will not contain PII.
					Serilog.Log.Logger.Error(ex2, "Error getting download license");
					throw;
				}
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

        private static readonly string[] CodecPreferenceOrder = new[] { EnhancedCodec.Lc128_44100_Stereo, EnhancedCodec.Lc64_44100_Stereo, EnhancedCodec.Lc64_22050_Stereo, EnhancedCodec.Lc32_22050_Stereo, EnhancedCodec.Aax, EnhancedCodec.Mp444128, EnhancedCodec.Mp44464, EnhancedCodec.Mp42264, EnhancedCodec.Mp42232, EnhancedCodec.Piff44128, EnhancedCodec.Piff4464, EnhancedCodec.Piff2232, EnhancedCodec.Piff2264 };

		/// <summary>
		/// download aax book file.
		/// note that this is always a single-file download, even with normally multi-part books
		/// 
		/// this is the 'aax workaround' to the AAXC problem
		/// https://github.com/mkb79/Audible/issues/3
		/// </summary>
		/// <returns>Filename of downloaded file</returns>
		public async Task<string> DownloadAaxWorkaroundAsync(string asin)
		{
			#region // downloading via cds.audible.com
			// there are probably other options to play with later.
			// here's what i'd previously scraped from the library page:
			// https://cds.audible.com/download/admhelper?user_id=xsIBLaVXKZWXlo5Azn5OfKIpHO_aES2Obo0Pq4JbIyJgzpJB7vpW-_GtV7_a&product_id=BK_TANT_004502&domain=www.audible.com&cust_id=xsIBLaVXKZWXlo5Azn5OfKIpHO_aES2Obo0Pq4JbIyJgzpJB7vpW-_GtV7_a&DownloadType=Now&transfer_player=1&codec=LC_64_22050_Stereo&awtype=AAX&title=The Utterly Uninteresting and Unadventurous Tales of Fred, the Vampire Accountant
			// here's what the other api makers discovered as their aax workaround around the aaxc issue:
			// https://cds.audible.com/download?asin=B06WLMWF2S&cust_id=zLKehcc-_2JBt-P-KqVn4VoWpfv3fqLyDVJ5SBPqXD1lCzjHSSzHBtW1I2wd&codec=LC_64_22050_stereo&source=audible_iPhone&type=AUDI
			#endregion

			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			var allCodecs = await getAllCodecsAsync(asin);
			// since Intersect() doesn't guarantee order, do not use it here
			var codec = CodecPreferenceOrder.FirstOrDefault(allCodecs.Contains) ?? allCodecs[0];

			// note: remainder of this method requires a DIFFERENT client
			var client = Sharer.GetSharedHttpClient("https://cde-ta-g7g.amazon.com");

			var requestUri = $"/FionaCDEServiceEngine/FSDownloadContent?type=AUDI&currentTransportMethod=WIFI&key={asin}&codec={codec}";
			var response = await AdHocAuthenticatedGetAsync(requestUri, client);
			if (response.StatusCode != HttpStatusCode.Found)
				throw new Exception("Incorrect response code");
			var downloadUrl = response.Headers.Location.AbsoluteUri;

			// localize download link
			var cdsRoot = "https://cds.audible.";
			downloadUrl = downloadUrl.Replace(
				$"{cdsRoot}com",
				$"{cdsRoot}{Locale.TopDomain}");

			return downloadUrl;
		}

		private async Task<List<string>> getAllCodecsAsync(string asin)
		{
			const int maxAttempts = 2;

			string bookJson;

			var libraryOptions = LibraryOptions.ResponseGroupOptions.ProductAttrs | LibraryOptions.ResponseGroupOptions.Relationships;

			var currAttempt = 0;
			do
			{
				currAttempt++;

				var bookJObj = await GetLibraryBookAsync(asin, libraryOptions);
				bookJson = bookJObj.ToString();
				var availableCodecs = bookJObj?.AvailableCodecs;
				var codecs = availableCodecs?
					.Where(ac => !string.IsNullOrWhiteSpace(ac.EnhancedCodec))
					.Select(ac => ac.EnhancedCodec)
					.ToList();

				if (codecs is not null && codecs.Any())
					return codecs;

				// if no codec, try once more with all options enabled
				libraryOptions = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS;
			}
			while (currAttempt < maxAttempts);

			// if still no codec: error
			throw new ApplicationException("Book has no codec specified. Cannot continue with download. Full book data:\r\n" + bookJson);
		}

		public async Task<string> GetPdfDownloadLinkAsync(string asin) => (await GetDownloadLicenseAsync(asin)).PdfUrl;
    }
}
