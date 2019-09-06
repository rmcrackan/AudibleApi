using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
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
			validateNotBlank(asin, nameof(asin));
			validateNotBlank(file, nameof(file));

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
			validateNotBlank(asin, nameof(asin));

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
			validateNotBlank(file, nameof(file));

            var downloadLink = await GetDownloadLinkAsync(asin);
            if (downloadLink == null)
                return null;

            // fix extension
            file = PathLib.GetPathWithExtensionFromAnotherFile(file, downloadLink);

            // download file
            await _client.DownloadFileAsync(downloadLink, file, progress);

            return file;
        }

        /// <returns>Return download link if successful. null if denied</returns>
        public async Task<string> GetDownloadLinkAsync(string asin)
        {
			//
			// this method has the potential to return files of the new .aaxc format which is currently un-broken and cannot be decrypted
			//

			validateNotBlank(asin, nameof(asin));

            var body = new JObject
            {
                { "consumption_type", "Download" },
                { "drm_type", "Adrm" },
                { "quality", "Extreme" }
            };
            var url = $"{CONTENT_PATH}/{asin}/licenserequest";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.AddContent(body);
            await signRequestAsync(request);

            var response = await _client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            var obj = JObject.Parse(responseString);

            // if we get "message" on this level means something went wrong.
            // "message" should be nested under "content_license"
            if (obj.TryGetValue("message", out var val))
            {
                var responseMessage = val.Value<string>();
                throw new ApiErrorException(response.Headers.Location, new JObject { { "error", responseMessage } });
            }

            var status_code = obj["content_license"]["status_code"].Value<string>();
            if (status_code.EqualsInsensitive("Denied"))
                return null;

            if (!status_code.EqualsInsensitive("Granted"))
                throw new ApiErrorException(response.Headers.Location, new JObject { { "error", "Unexpected status_code: " + status_code } });

            var link = obj["content_license"]["content_metadata"]["content_url"]["offline_url"].Value<string>();
            return link;
		}

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

			validateNotBlank(asin, nameof(asin));
			validateNotBlank(destinationFilePath, nameof(destinationFilePath));

			// can also get codec options from /1.0/library/{asin}?response_groups=product_attrs,relationships
			var codec = "LC_64_22050_stereo";

			// note: this method requires a DIFFERENT client
			var client = _sharer.GetSharedClient(
				new Uri("https://cde-ta-g7g.amazon.com"));

			// REQUEST 1: GET DOWNLOAD LINK
			var requestUri = $"/FionaCDEServiceEngine/FSDownloadContent?type=AUDI&currentTransportMethod=WIFI&key={asin}&codec={codec}";
			var response = await AdHocAuthenticatedGetAsync(requestUri, client);
			if (response.StatusCode != HttpStatusCode.Found)
				throw new Exception("Incorrect response code");
			var downloadUrl = response.Headers.Location.AbsoluteUri;

			// localize download link
			var cdsRoot = "https://cds.audible.";
			downloadUrl = downloadUrl.Replace(
				$"{cdsRoot}com",
				$"{cdsRoot}{Localization.CurrentLocale.Domain}");

			// REQUEST 2: DOWNLOAD FILE
			var filename = await client.DownloadFileAsync(downloadUrl, destinationFilePath, progress);

			return filename;
		}

		private static void validateNotBlank(string value, string paramName)
		{
			if (value is null)
				throw new ArgumentNullException(paramName);
			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentException();
		}
	}
}
