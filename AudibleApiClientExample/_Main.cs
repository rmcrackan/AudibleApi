using System;
using System.IO;
using System.Threading.Tasks;

namespace AudibleApiClientExample
{
    class _Main
	{
		// store somewhere that can't accidentally be added to git
		static string loginFilePath => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
			"SECRET LOGIN.txt");

		static async Task Main(string[] args)
		{
			fixPathInAppSettings();

			var (email, password) = getCredentials();

			var client = await AudibleApiClient.CreateClientAsync(email, password);
            await client.PrintLibraryAsync();
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

		static (string email, string password) getCredentials()
		{
			if (File.Exists(loginFilePath))
			{
				var pwParts = File.ReadAllLines(loginFilePath);
				var email = pwParts[0];
				var password = pwParts[1];

				if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
					return (email, password);
			}

			Console.WriteLine("Email:");
			var e = Console.ReadLine().Trim();
			Console.WriteLine("Password:");
			var pw = Dinah.Core.ConsoleLib.ConsoleExt.ReadPassword();
			return (e, pw);
		}
	}
}
