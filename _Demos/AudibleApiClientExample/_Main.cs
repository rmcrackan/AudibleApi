using System;
using System.IO;
using System.Threading.Tasks;

namespace AudibleApiClientExample
{
    class _Main
	{
		// store somewhere that can't accidentally be added to git
		public static string loginFilePath => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
			"SECRET LOGIN.txt");

		static async Task Main(string[] args)
		{
			// TO USE EXISTING IdentityTokens.json
			// run this app once to copy appsettings.json to build dir
			// open appsettings.json in build dir
			// find location of existing IdentityTokens.json
			// put this location into appsettings.json in build dir. remember to escape back-slashes
			// save appsettings.json

			var settings = File.ReadAllText("appsettings.json");
			var jObj = Newtonsoft.Json.Linq.JObject.Parse(settings);
			var IdentityFilePath = jObj.Value<string>("IdentityFilePath");
			var client = await AudibleApiClient.CreateClientAsync(IdentityFilePath);

			//// use client
			//await client.PrintLibraryAsync();
			//await client.DownloadBookAsync();
			//await client.DocumentLibraryResponseGroupOptionsAsync();
			await client.DeserializeSingleBookAsync();
		}
	}
}
