using System;
using System.Collections.Generic;
using System.IO;
using AudibleApi.Authorization;
using Newtonsoft.Json.Linq;

namespace L1.Tests
{
    public static class REAL
    {
		const string APP_SETTINGS = @"L1\appsettings.json";
		public static string TokenFilePath
		{
			get
			{
				try
				{
					var contents = File.ReadAllText(APP_SETTINGS);
					var jObject = JObject.Parse(contents);
					var identityFilePath = jObject["IdentityFilePath"];
					var path = identityFilePath.Value<string>();
					if (!File.Exists(path))
					{
						path = Path.Combine("L1", path);
						if (!File.Exists(path))
							throw new FileNotFoundException();
					}
					return path;
				}
				catch
				{
					var lines = new List<string>
					{
						$"Error! {nameof(APP_SETTINGS)} not found.\r\nTo fix this error, copy the client's appsettings IdentityFilePath entry into the L1's appsettings IdentityFilePath entry. If Libation is on this computer, the path is probably %LibationFiles%\\IdentityTokens.json"
					};

					throw new Exception(string.Join("\r\n", lines));
				}
			}
		}

		public static Identity GetIdentity()
			=> Identity.FromJson(File.ReadAllText(TokenFilePath));
	}
}
