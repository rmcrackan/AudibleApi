using System;
using System.Linq;

namespace AudibleApi.Common
{
	public partial class Item : DtoBase<Item>
	{
		public override string ToString() => $"[{ProductId}] {Title}";

		public string ProductId => Asin;
		public int LengthInMinutes => RuntimeLengthMin ?? 0;
		public string Description => PublisherSummary;
		public bool IsEpisodes
			=> Relationships?.Any(r => r.RelationshipToProduct == RelationshipToProduct.Parent && r.RelationshipType == RelationshipType.Episode)
			?? false;
		public bool IsSeriesParent
			=> Relationships is not null
			&& Relationships.Any(r => r.RelationshipToProduct == RelationshipToProduct.Child && r.RelationshipType == RelationshipType.Episode)
			&& !Relationships.Any(r => r.RelationshipToProduct == RelationshipToProduct.Parent && r.RelationshipType == RelationshipType.Season);

		public string PictureId => ProductImages?.PictureId;
		public string PictureLarge => ProductImages?.PictureLarge;
		public DateTime DateAdded => PurchaseDate.UtcDateTime;

		public float Product_OverallStars => Convert.ToSingle(Rating?.OverallDistribution?.DisplayStars ?? 0);
		public float Product_PerformanceStars => Convert.ToSingle(Rating?.PerformanceDistribution?.DisplayStars ?? 0);
		public float Product_StoryStars => Convert.ToSingle(Rating?.StoryDistribution?.DisplayStars ?? 0);

		public int MyUserRating_Overall => Convert.ToInt32(ProvidedReview?.Ratings?.OverallRating ?? 0L);
		public int MyUserRating_Performance => Convert.ToInt32(ProvidedReview?.Ratings?.PerformanceRating ?? 0L);
		public int MyUserRating_Story => Convert.ToInt32(ProvidedReview?.Ratings?.StoryRating ?? 0L);

		/// <summary>Indicates if this item may be added to the user's library.</summary>
		public bool CanAddToLibrary => Price is not null;
		public bool IsAbridged => FormatType == AudibleApi.Common.FormatType.Abridged;
		public DateTime? DatePublished => PublicationDateTime?.UtcDateTime ?? IssueDate?.UtcDateTime; // item.IssueDate == item.ReleaseDate
		public string Publisher => PublisherName;

		// these category properties assume:
		// - we're only exposing 1 category, irrespective of how many the Item actually has
		// - each ladder will have either 1 or 2 levels: parent and optional child
		public Ladder[] Categories => CategoryLadders?.FirstOrDefault()?.Ladder ?? new Ladder[0];
		public Ladder ParentCategory => Categories?.FirstOrDefault();
		public Ladder ChildCategory => Categories.Length > 1 ? Categories[1] : null;

		/// <summary>Add any subtitle after the title title</summary>
		public string TitleWithSubtitle
		{
			get
			{
				var title = Title?.Trim();
				var subtitle = Subtitle?.Trim();
				return !string.IsNullOrWhiteSpace(subtitle)
					? $"{title}: {subtitle}"
					: title;
			}
		}
	}
}
