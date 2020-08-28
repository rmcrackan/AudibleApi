using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AudibleApi.Authorization;
using InternalUtilities;

namespace L1.Tests
{
    public static class REAL
    {
		private static string _tokenFilePath
		{
			get
			{
				try
				{
					return AudibleApiStorage.AccountsSettingsFile;
				}
				catch (Exception ex)
				{
					throw new Exception($"Error! settings file not found.\r\nTo fix this error, copy the client's appsettings LibationFiles path into the L1's appsettings.json LibationFiles entry", ex);
				}
			}
		}

		public static string JsonPath
			=> AudibleApiStorage
				.GetAccountsSettingsPersister()
				.AccountsSettings
				.GetAll()
				.FirstOrDefault()
				.GetIdentityTokensJsonPath();

		public static Identity GetIdentity()
			=> Identity.FromJson(File.ReadAllText(_tokenFilePath), JsonPath);

		public static IdentityPersister GetIdentityPersister()
			=> new IdentityPersister(_tokenFilePath, JsonPath);
	}
}
