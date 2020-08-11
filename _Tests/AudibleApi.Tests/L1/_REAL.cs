using System;
using System.Collections.Generic;
using System.IO;
using AudibleApi.Authorization;

namespace L1.Tests
{
    public static class REAL
    {
		public static string TokenFilePath
		{
			get
			{
				try
				{
					return FileManager.AudibleApiStorage.AccountsSettingsFile;
				}
				catch (Exception ex)
				{
					throw new Exception($"Error! settings file not found.\r\nTo fix this error, copy the client's appsettings LibationFiles path into the L1's appsettings LibationFiles entry");
				}
			}
		}

		public static Identity GetIdentity()
			=> Identity.FromJson(File.ReadAllText(TokenFilePath));
	}
}
