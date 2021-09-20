using System;
using System.IO;
using System.Threading.Tasks;
using AudibleApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApiClientExample
{
	public record Secrets(string email, string password, string jsonPath, int accountIndex);

	public static class Program
	{
		public static Secrets GetSecrets()
		{
			// store somewhere that can't accidentally be added to git
			var secretsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "SECRET.txt");
			if (!File.Exists(secretsPath))
				return null;

			try
			{
				var pwParts = File.ReadAllLines(secretsPath);

				var acctIndex
					= pwParts.Length <= 3 ? 0
					: int.Parse(pwParts[3]) - 1; // change 1-based to 0-based index

				return new Secrets(pwParts[0], pwParts[1], pwParts[2], acctIndex);
			}
			catch
			{
				return null;
			}
		}

		static async Task Main(string[] args)
		{
			try
			{
				await UserSetup.Run();
			}
			catch (AudibleApiException aex)
			{
				Console.WriteLine("ERROR:");
				Console.WriteLine(aex.Message);
				Console.WriteLine(aex.JsonMessage.ToString());
				Console.WriteLine(aex.RequestUri.ToString());
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR:");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}

		public async static Task<AudibleApiClient> CreateClientAsync()
		{
			var locale = UserSetup.LOCALE_NAME;
			var identityFilePath = UserSetup.IDENTITY_FILE_PATH;
			var jsonPath = UserSetup.JSON_PATH;

			var secrets = Program.GetSecrets();
			if (secrets is not null)
			{
				try
				{
					var accountsSettingsJsonPath = secrets.jsonPath;
					var accountsSettingsJson = File.ReadAllText(accountsSettingsJsonPath);
					var jObj = JObject.Parse(accountsSettingsJson);

					var acctSettingsJsonPath = $"$.Accounts[{secrets.accountIndex}].IdentityTokens";

					var localeName = jObj.SelectToken(acctSettingsJsonPath + ".LocaleName").Value<string>();

					// success. set var.s
					locale = localeName;
					identityFilePath = accountsSettingsJsonPath;
					jsonPath = acctSettingsJsonPath;
				}
				catch { }
			}

			var api = await EzApiCreator.GetApiAsync(
				new LoginCallback(),
				Localization.Get(locale),
				identityFilePath,
				jsonPath);

			return new AudibleApiClient(api);
		}
	}
}
