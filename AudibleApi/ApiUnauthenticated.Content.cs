using AudibleApi.Common;
using Dinah.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AudibleApi
{
    public partial class ApiUnauthenticated
	{
        protected const string CONTENT_PATH = "/1.0/content";
		public async Task<ContentMetadata> GetContentMetadataAsync(string asin)
		{
			asin = ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin)).ToUpper().Trim();

			var url = $"{CONTENT_PATH}/{asin}/metadata?response_groups=chapter_info,content_reference";

			var response = await AdHocNonAuthenticatedGetAsync(url);
			var metadataJson = await response.Content.ReadAsStringAsync();

			MetadataDtoV10 contentMetadata;
			try
			{
				contentMetadata = MetadataDtoV10.FromJson(metadataJson);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"Error retrieving content metadata for asin: {asin}");
				throw;
			}
			return contentMetadata?.ContentMetadata;
		}
	}
}
