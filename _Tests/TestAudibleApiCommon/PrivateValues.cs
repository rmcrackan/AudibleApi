using System;
using System.IO;

namespace TestAudibleApiCommon
{
	/// <summary>
	/// values that range from 'contains benign personal identifying information' to 'entirely secret'
	/// </summary>
	public static class PrivateValues
	{
		private static string _LibraryWithOptions_file = "LibraryWithOptions.json";

		public static bool LibraryWithOptionsExists => File.Exists(_LibraryWithOptions_file);
		/// <summary>
		/// NumberOfResultPerPage = 20
		/// PageNumber = 5
		/// PurchasedAfter = new DateTime(1970, 1, 1)
		/// ResponseGroups = Sku | Reviews
		/// SortBy = TitleDesc
		/// </summary>
		public static string LibraryWithOptions
		{
			get
			{
				if (!LibraryWithOptionsExists)
					throw new Exception($"test file [{nameof(_LibraryWithOptions_file)}] does not exist. run L1 test to create");
				var contents = File.ReadAllText(_LibraryWithOptions_file);
				return contents;
			}
		}
		public static void WriteLibraryWithOptions(string value)
			=> File.WriteAllText(_LibraryWithOptions_file, value);
	}
}