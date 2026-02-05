using AudibleApi.Common;
using Dinah.Core;
using System.Text;
using System.Threading.Tasks;

namespace AudibleApi;

public partial class ApiUnauthenticated
{
	protected const string CONTENT_PATH = "/1.0/content";
	public async Task<ContentMetadata?> GetContentMetadataAsync(string asin, ChapterTitlesType chapterTitlesType = ChapterTitlesType.Tree)
		=> await GetContentMetadataAsync(asin, null, null, null, chapterTitlesType);

	public async Task<ContentMetadata?> GetContentMetadataAsync(string asin, DrmType? drmType, string? acr, long? fileVersion, ChapterTitlesType chapterTitlesType = ChapterTitlesType.Tree)
	{
		asin = ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin)).ToUpper().Trim();
		if (drmType.HasValue != acr is not null)
			throw new System.ArgumentException($"{nameof(drmType)} and {nameof(acr)} must either both be null or both contain values.");

		var url = $"{CONTENT_PATH}/{asin}/metadata?response_groups=chapter_info,content_reference&chapter_titles_type={chapterTitlesType}";
		var sb = new StringBuilder(url);
		if (drmType.HasValue)
		{
			sb.Append($"&acr={acr}");
			sb.Append($"&drm_type={drmType.Value}");
		}
		if (fileVersion.HasValue)
			sb.Append($"&file_version={fileVersion.Value}");

		var response = await AdHocNonAuthenticatedGetAsync(sb.ToString());
		var contentMetadata = await response.Content.ReadAsDtoAsync<MetadataDtoV10>();

		return contentMetadata?.ContentMetadata;
	}
}
