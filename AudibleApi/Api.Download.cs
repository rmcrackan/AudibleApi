﻿using System;
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

		/// <summary>
		/// download all book parts
		/// </summary>
		/// <param name="asin"></param>
		/// <param name="file">desired full path, file name, incl extension</param>
		/// <returns>
		/// Actual filenames. If needed, extension will be derived from the download file.
		/// Eg:
		///   file=foo.abc
		///   downloadfile=aaa.xyz
		///   return=foo.xyz
		/// 
		/// For multple parts, files will be appended part number
		/// Eg:
		///   file=foo.abc
		///   2 parts=aaa.xyz
		///   return= { foo(1).xyz , foo(2).xyz }
		/// </returns>
		public async Task<IEnumerable<string>> DownloadAsync(string asin, string file, IProgress<DownloadProgress> progress = null)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));
			ArgumentValidator.EnsureNotNullOrWhiteSpace(file, nameof(file));

			var asins = (await GetDownloadablePartsAsync(asin)).ToList();

			if (asins.Count == 1)
			{
				var single = await DownloadPartAsync(asin, file);
				return new List<string> { single };
			}

			var filepart = Path.Combine(
				Path.GetDirectoryName(file),
				Path.GetFileNameWithoutExtension(file));

			// includes dot char
			var ext = Path.GetExtension(file);

			// eg:
			//   1-9 parts: pad to 1 digit. ie: no pad
			//   10-99 parts: pad to 2 digits. ie: 1 leading zero
			var asinsCountChar = asins.Count.ToString().Length;

			var list = new List<string>();

			for (var i = 0; i < asins.Count; i++)
			{
				var asinInOrder = asins[i];
				var num = (i + 1)
					.ToString()
					.PadLeft(asinsCountChar, '0');
				var filename = $"{filepart}({num}){ext}";
				var part = await DownloadPartAsync(asinInOrder, filename, progress);
				list.Add(part);
			}

			return list;
		}
		/// <summary>
		/// For the provided ASIN, get the ASINs of all parts to download. Parts are in order
		/// </summary>
		/// <returns>All ASINs to download, in order</returns>
		public async Task<IEnumerable<string>> GetDownloadablePartsAsync(string asin)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			var bookDetails = await GetLibraryBookAsync(asin, LibraryOptions.ResponseGroupOptions.Relationships);

			var debug = bookDetails.ToString(Formatting.Indented);

			var orderedChildrenAsins
				= bookDetails["item"]["relationships"]
				.ToArray()
				.Where(r => (string)r["relationship_to_product"] == "child")
				.OrderBy(r => int.Parse((string)r["sort"]))
				.Select(r => (string)r["asin"])
				.ToList();

			if (orderedChildrenAsins.Any())
				return orderedChildrenAsins;

			return new List<string> { asin };
		}

		/// <summary>
		/// download single file part. for small books, this is all that's needed. for safety, use DownloadAsync
		/// </summary>
		/// <param name="asin"></param>
		/// <param name="file">desired full path, file name, incl extension</param>
		/// <returns>Actual filename. If needed, extension will be derived from the download file.
		/// Eg: file=foo.abc, downloadfile=bar.xyz, return=foo.xyz</returns>
		public async Task<string> DownloadPartAsync(string asin, string file, IProgress<DownloadProgress> progress = null)
        {
			ArgumentValidator.EnsureNotNullOrWhiteSpace(file, nameof(file));

            var downloadLicense = await GetDownloadLicenseAsync(asin);

			var downloadLink = downloadLicense?.ContentMetadata?.ContentUrl?.OfflineUrl;

			if (downloadLink == null)
                return null;

            // fix extension
            file = PathLib.GetPathWithExtensionFromAnotherFile(file, downloadLink);

            // download file
            await _client.DownloadFileAsync(downloadLink, file, progress);

            return file;
        }


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
				return null;

			if (contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Denied"))
				return null;

			if (!contentLicenseDtoV10.ContentLicense.StatusCode.EqualsInsensitive("Granted"))
				throw new ApiErrorException(response.Headers.Location, new JObject { { "error", "Unexpected status_code: " + contentLicenseDtoV10.ContentLicense.StatusCode } });

			return contentLicenseDtoV10.ContentLicense;
		}

        #endregion


        /// <summary>
        /// download aax book file.
        /// note that this is always a single-file download, even with normally multi-part books
        /// 
        /// this is the 'aax workaround' to the AAXC problem
        /// https://github.com/mkb79/Audible/issues/3
        /// </summary>
        /// <returns>Filename of downloaded file</returns>
        public async Task<string> DownloadAaxWorkaroundAsync(string asin, string destinationFilePath, IProgress<DownloadProgress> progress = null)
		{
			#region // downloading via cds.audible.com
			// there are probably other options to play with later.
			// here's what i'd previously scraped from the library page:
			// https://cds.audible.com/download/admhelper?user_id=xsIBLaVXKZWXlo5Azn5OfKIpHO_aES2Obo0Pq4JbIyJgzpJB7vpW-_GtV7_a&product_id=BK_TANT_004502&domain=www.audible.com&cust_id=xsIBLaVXKZWXlo5Azn5OfKIpHO_aES2Obo0Pq4JbIyJgzpJB7vpW-_GtV7_a&DownloadType=Now&transfer_player=1&codec=LC_64_22050_Stereo&awtype=AAX&title=The Utterly Uninteresting and Unadventurous Tales of Fred, the Vampire Accountant
			// here's what the other api makers discovered as their aax workaround around the aaxc issue:
			// https://cds.audible.com/download?asin=B06WLMWF2S&cust_id=zLKehcc-_2JBt-P-KqVn4VoWpfv3fqLyDVJ5SBPqXD1lCzjHSSzHBtW1I2wd&codec=LC_64_22050_stereo&source=audible_iPhone&type=AUDI
			#endregion

			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));
			ArgumentValidator.EnsureNotNullOrWhiteSpace(destinationFilePath, nameof(destinationFilePath));

			var codec = await GetCodecAsync(asin);

			// note: remainder of this method requires a DIFFERENT client
			var client = Sharer.GetSharedHttpClient("https://cde-ta-g7g.amazon.com");

			var downloadUrl = await GetDownloadLinkAsync(client, asin, codec);

			var filename = await client.DownloadFileAsync(downloadUrl, destinationFilePath, progress);

			return filename;
		}

		public async Task<string> GetCodecAsync(string asin)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			var codecs = await getAllCodecsAsync(asin);

			var codecPreferenceOrder = new[] { EnhancedCodec.Lc128_44100_Stereo, EnhancedCodec.Lc64_44100_Stereo, EnhancedCodec.Lc64_22050_Stereo, EnhancedCodec.Lc32_22050_Stereo, EnhancedCodec.Aax, EnhancedCodec.Mp444128, EnhancedCodec.Mp44464, EnhancedCodec.Mp42264, EnhancedCodec.Mp42232, EnhancedCodec.Piff44128, EnhancedCodec.Piff4464, EnhancedCodec.Piff2232, EnhancedCodec.Piff2264 };

			// since Intersect() doesn't guarantee order, do not use it here
			var codec = codecPreferenceOrder.FirstOrDefault(p => codecs.Contains(p));

			// found a codec among this list
			if (codec is not null)
				return codec;

			// else return first available
			return codecs[0];
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
				bookJson = bookJObj.ToString(Formatting.Indented);
				var availableCodecs = BookDtoV10.FromJson(bookJson)?.Item?.AvailableCodecs;
				var codecs = availableCodecs?
					.Where(ac => !string.IsNullOrWhiteSpace(ac.EnhancedCodec))
					.Select(ac => ac.EnhancedCodec)
					.ToList();

				if (codecs is not null && codecs.Any())
					return codecs;

				// if no codec, try once more with all options enbled
				libraryOptions = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS;
			}
			while (currAttempt < maxAttempts);

			// if still no codec: error
			throw new ApplicationException("Book has no codec specified. Cannot continue with download. Full book data:\r\n" + bookJson);
		}

		public async Task<string> GetDownloadLinkAsync(IHttpClientActions client, string asin, string codec)
		{
			ArgumentValidator.EnsureNotNull(client, nameof(client));
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));
			ArgumentValidator.EnsureNotNullOrWhiteSpace(codec, nameof(codec));

			var requestUri = $"/FionaCDEServiceEngine/FSDownloadContent?type=AUDI&currentTransportMethod=WIFI&key={asin}&codec={codec}";
			var response = await AdHocAuthenticatedGetAsync(requestUri, client);
			if (response.StatusCode != HttpStatusCode.Found)
				throw new Exception("Incorrect response code");
			var downloadUrl = response.Headers.Location.AbsoluteUri;

			// localize download link
			var cdsRoot = "https://cds.audible.";
			downloadUrl = downloadUrl.Replace(
				$"{cdsRoot}com",
				$"{cdsRoot}{_locale.TopDomain}");

			return downloadUrl;
		}

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
