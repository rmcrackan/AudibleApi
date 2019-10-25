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
			fixPathInAppSettings();

			var client = await AudibleApiClient.CreateClientAsync();

			//// use client
			//await client.PrintLibraryAsync();
			//await client.DownloadBookAsync();
			//await client.DocumentLibraryResponseGroupOptionsAsync();
		}

		static void fixPathInAppSettings()
		{
			// store a relative path in the version-controled file. when run locally, this method will replace it with the absolute path
			var settings = ClientSettings.FromFile(AudibleApiClient.APP_SETTINGS);
			var fullPath = Path.GetFullPath(settings.IdentityFilePath);
			if (fullPath != settings.IdentityFilePath)
			{
				// ClientSettings is special. setting a property will also save the new value to file; no other action needed
				settings.IdentityFilePath = fullPath;
			}
		}
	}
}
