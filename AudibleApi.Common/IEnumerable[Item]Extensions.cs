using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AudibleApi.Common;

public static class IEnumerable_Item_Extensions
{
	extension(IEnumerable<Item>? items)
	{
		[return: NotNullIfNotNull(nameof(items))]
		public IEnumerable<Item>? NonNull() => items?.OfType<Item>();

		[return: NotNullIfNotNull(nameof(items))]
		public IEnumerable<Person>? GetAuthorsDistinct()
			=> items.NonNull()
			?.SelectMany(i => i.Authors ?? [])
			.OfType<Person>()
			.DistinctBy(a => new { a.Name, a.Asin });

		[return: NotNullIfNotNull(nameof(items))]
		public IEnumerable<Person>? GetNarratorsDistinct()
			=> items.NonNull()
			?.SelectMany(i => i.Narrators ?? [])
			.DistinctBy(n => new { n.Name, n.Asin });

		[return: NotNullIfNotNull(nameof(items))]
		public IEnumerable<string>? GetNarratorNamesDistinct()
			=> items.NonNull()
			?.SelectMany(i => i.Narrators ?? [], (i, n) => n.Name)
			.OfType<string>()
			.Distinct();

		[return: NotNullIfNotNull(nameof(items))]
		public IEnumerable<string>? GetPublishersDistinct()
			=> items.NonNull()
			?.Select(i => i.Publisher)
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.Cast<string>()
			.Distinct();

		[return: NotNullIfNotNull(nameof(items))]
		public IEnumerable<Series>? GetSeriesDistinct()
			=> items.NonNull()
			?.SelectMany(i => i.Series ?? [])
			// if series is present, Name and Id must both be non-null. don't use Elvis operator in DistinctBy()
			.DistinctBy(s => new { s.SeriesName, s.SeriesId });

		[return: NotNullIfNotNull(nameof(items))]
		public IEnumerable<Ladder?[]>? GetCategoryPairsDistinct()
			=> items.NonNull()
			?.Where(i => !string.IsNullOrWhiteSpace(i.ParentCategory?.Name))
			.DistinctBy(i => new
			{
				parentName = i.ParentCategory?.Name,
				parentId = i.ParentCategory?.Id,
				childName = i.ChildCategory?.Name,
				childId = i.ChildCategory?.Id
			})
			.Select(i => i.Categories)
			.OfType<Ladder?[]>()
			.Where(c => c.Length > 0);

		[return: NotNullIfNotNull(nameof(items))]
		public IEnumerable<Ladder>? GetCategoriesDistinct()
			=> items.NonNull()
			?.SelectMany(l => l.Categories ?? [])
			.OfType<Ladder>()
			.DistinctBy(l => new { l.CategoryName, l.CategoryId })
			.Where(l => !string.IsNullOrWhiteSpace(l.CategoryName));
	}
}
