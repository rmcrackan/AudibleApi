using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Dinah.Core.Net.Http
{
	public class DownloadProgress
	{
		public long BytesReceived { get; set; }
		public long? TotalFileSize { get; set; }
		public double? ProgressPercentage { get; set; }
	}

	public static class SystemNetHttpExtensions
	{
		// Posting URL-encoded key values with HttpClient
		// http://ronaldrosiernet.azurewebsites.net/Blog/2013/12/07/posting_urlencoded_key_values_with_httpclient

		public static void AddContent(this HttpRequestMessage request, JObject jObj)
			=> request.AddContent(new StringContent(jObj.ToString(), System.Text.Encoding.UTF8, "application/json"));

		public static void AddContent(this HttpRequestMessage request, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
			=> request.AddContent(new FormUrlEncodedContent(keyValuePairs));

		public static void AddContent(this HttpRequestMessage request, HttpContent content)
			=> request.Content = content;

		public static Cookie ParseCookie(string cookieString)
		{
			if (cookieString == null)
				throw new ArgumentNullException(nameof(cookieString));

			var parts = cookieString
				.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(c => c.Trim())
				.ToList();

			var cookie = new Cookie();

			foreach (var part in parts)
			{
				var kvp = part
					.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(c => c.Trim())
					.ToList();
				var key = kvp[0];
				var value = (kvp.Count == 2) ? kvp[1] : null;
				switch (key.ToLower())
				{
					case "secure":
						cookie.Secure = true;
						break;
					case "path":
						cookie.Path = value;
						break;
					case "domain":
						cookie.Domain = value;
						break;
					case "expires":
						cookie.Expires = DateTime.Parse(value);
						break;
					default:
						cookie.Name = key;
						cookie.Value = value;
						break;
				}
			}

			return cookie;
		}

		public static async Task<JObject> ReadAsJObjectAsync(this HttpContent content)
		{
			if (content is FormUrlEncodedContent formUrlEncodedContent)
				return await formUrlEncodedContent.ReadAsJObjectAsync();

			var str = await content.ReadAsStringAsync();
			return JObject.Parse(str);
		}

		public static async Task<JObject> ReadAsJObjectAsync(this FormUrlEncodedContent content)
		{
			//
			// IMPORTANT
			// duplicate keys will be lost.
			// a NameValueCollection allows duplicates, unlike a KeyValuePair
			//
			var str = await content.ReadAsStringAsync();
			var nameValueCollection = System.Web.HttpUtility.ParseQueryString(str);
			var dic = nameValueCollection.AllKeys.ToDictionary(k => k, k => nameValueCollection[k]);

			var jObj = JObject.FromObject(dic);
			var debugJson = jObj.ToString(Newtonsoft.Json.Formatting.Indented);
			return jObj;
		}

		// http://www.tugberkugurlu.com/archive/efficiently-streaming-large-http-responses-with-httpclient
		// use ResponseHeadersRead
		public static async Task<string> DownloadFileAsync(
			this ISealedHttpClient client,
			string downloadUrl,
			string destinationFilePath,
			IProgress<DownloadProgress> progress = null)
		{
			downloadValidate(client, downloadUrl, destinationFilePath);

			using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
				return await downloadAsync(response, destinationFilePath, progress);
		}
		public static async Task<string> DownloadFileAsync(
			this HttpClient client,
			string downloadUrl,
			string destinationFilePath,
			IProgress<DownloadProgress> progress = null)
		{
			downloadValidate(client, downloadUrl, destinationFilePath);

			using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
				return await downloadAsync(response, destinationFilePath, progress);
		}
		private static void downloadValidate(object client, string downloadUrl, string fileToWriteTo)
		{
			if (client is null)
				throw new ArgumentNullException(nameof(client));
			if (downloadUrl is null)
				throw new ArgumentNullException(nameof(downloadUrl));
			if (fileToWriteTo is null)
				throw new ArgumentNullException(nameof(fileToWriteTo));
			if (string.IsNullOrWhiteSpace(downloadUrl))
				throw new ArgumentException(nameof(downloadUrl) + " may not be blank");
			if (string.IsNullOrWhiteSpace(fileToWriteTo))
				throw new ArgumentException(nameof(fileToWriteTo) + " may not be blank");
		}
		private static async Task<string> downloadAsync(HttpResponseMessage response, string destinationFilePath, IProgress<DownloadProgress> progress)
		{
			response.EnsureSuccessStatusCode();

			// if ContentDisposition.FileName specifies file extension, then keep param file path and name but use file extension from ContentDisposition.FileName
			var headerFilename = response?.Content?.Headers?.ContentDisposition?.FileName;
			if (!string.IsNullOrWhiteSpace(headerFilename))
			{
				destinationFilePath = PathLib.GetPathWithExtensionFromAnotherFile(destinationFilePath, headerFilename);
			}

			using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
			using (var streamToWriteTo = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
			{
				if (progress == null)
				{
					await streamToReadFrom.CopyToAsync(streamToWriteTo);
					return destinationFilePath;
				}

				var totalFileSize = response.Content.Headers.ContentLength;
				var bytesReceived = 0L;
				var buffer = new byte[8192];

				while (true)
				{
					var bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length);
					if (bytesRead == 0)
						return destinationFilePath;

					await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);

					bytesReceived += bytesRead;

					reportProgress(bytesReceived, totalFileSize, progress);
				}
			}
		}

		private static void reportProgress(
			long bytesReceived,
			long? totalFileSize,
			IProgress<DownloadProgress> progress)
		{
			double? progressPercentage = null;
			if (totalFileSize.HasValue)
				progressPercentage = Math.Round((double)bytesReceived / totalFileSize.Value * 100, 2);

			var args = new DownloadProgress
			{
				BytesReceived = bytesReceived,
				TotalFileSize = totalFileSize,
				ProgressPercentage = progressPercentage
			};
			progress.Report(args);
		}
	}
}
