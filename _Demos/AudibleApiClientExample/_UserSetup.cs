using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Common;

namespace AudibleApiClientExample
{
	public static class UserSetup
	{
		// see locales.json for choices
		public const string LOCALE_NAME = "canada";

		// will be created if it does not exist
		public const string IDENTITY_FILE_PATH = "myIdentity.json";

		// if your json file is complex: you can specify the jsonPath within that file where identity is/should be stored.
		// else: null
		public const string JSON_PATH = null;

		// add your code to AudibleApiClient.cs and call it from here:
		public static async Task Run()
		{
			var client = await Program.CreateClientAsync();

			//await client.AccountInfoAsync();

			//AudibleApiClient.AnaylzeLibrary();

			//// get all books in library (page 1)
			//await client.PrintLibraryAsync();

			//// get book info from library
			//// book i own
			//await client.Api.GetLibraryBookAsync(AudibleApiClient.MEDIUM_BOOK_ASIN, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
			//// throws: not present in customer library
			//try { await client.Api.GetLibraryBookAsync(AudibleApiClient.DO_NOT_OWN_ASIN, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS); }
			//catch (Exception ex) { }

			//// get general book info. doesn't matter if in library. will return less info
			//await client.GetBookInfoAsync(AudibleApiClient.MEDIUM_BOOK_ASIN);
			//await client.GetBookInfoAsync(AudibleApiClient.DO_NOT_OWN_ASIN);

			//await client.PodcastTestsAsync();
		}
	}
}
