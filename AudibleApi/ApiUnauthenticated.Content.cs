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
		public async Task<ContentMetadata> GetContentMetadataAsync(string asin, ChapterTitlesType chapterTitlesType = ChapterTitlesType.Tree)
		{
			asin = ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin)).ToUpper().Trim();

			var url = $"{CONTENT_PATH}/{asin}/metadata?response_groups=chapter_info,content_reference&chapter_titles_type={chapterTitlesType}";

			var response = await AdHocNonAuthenticatedGetAsync(url);
			var contentMetadata = await response.Content.ReadAsDtoAsync<MetadataDtoV10>();
			
			return contentMetadata?.ContentMetadata;
		}
	}
}
