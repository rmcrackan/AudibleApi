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
			// TO USE EXISTING AccountsSettings.json
			// run this app once to copy appsettings.json to build dir
			// open appsettings.json in build dir
			// find location of existing AccountsSettings.json (prob in \LibationFiles)
			// put this location into appsettings.json in build dir. remember to escape back-slashes
			// save appsettings.json

			var client = await AudibleApiClient.CreateClientAsync();

			//// use client
			//await client.PrintLibraryAsync();
			//await client.DownloadBookAsync();
			//await client.DocumentLibraryResponseGroupOptionsAsync();
			//await client.DeserializeSingleBookAsync();
			await client.AccountInfoAsync();
		}
	}
}
