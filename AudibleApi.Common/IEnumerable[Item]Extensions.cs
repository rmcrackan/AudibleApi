using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core.Collections.Generic;

namespace AudibleApi.Common
{
	public static class IEnumerable_Item_Extensions
	{
		public static IEnumerable<Person> GetAuthorsDistinct(this IEnumerable<Item> items)
			=> items
			?.Where(i => i.Authors != null)
			.SelectMany(i => i.Authors)
			.DistinctBy(a => new { a.Name, a.Asin });

		public static IEnumerable<Person> GetNarratorsDistinct(this IEnumerable<Item> items)
			=> items
			?.Where(i => i.Narrators != null)
			.SelectMany(i => i.Narrators)
			.DistinctBy(n => new { n.Name, n.Asin });

		public static IEnumerable<string> GetNarratorNamesDistinct(this IEnumerable<Item> items)
			=> items
			?.Where(i => i.Narrators != null)
			.SelectMany(i => i.Narrators, (i, n) => n.Name)
			.Distinct();

		public static IEnumerable<string> GetPublishersDistinct(this IEnumerable<Item> items)
			=> items
			?.Where(i => !string.IsNullOrWhiteSpace(i.Publisher))
			.Select(i => i.Publisher)
			.Distinct();

		public static IEnumerable<Series> GetSeriesDistinct(this IEnumerable<Item> items)
			=> items
			?.Where(i => i.Series != null)
			.SelectMany(i => i.Series)
			// if series is present, Name and Id must both be non-null. don't use Elvis operator in DistinctBy()
			.DistinctBy(s => new { s.SeriesName, s.SeriesId });

		public static IEnumerable<Ladder[]> GetCategoryPairsDistinct(this IEnumerable<Item> items)
			=> items
			?.Where (i => !string.IsNullOrWhiteSpace(i.ParentCategory?.Name))
			.DistinctBy(i => new
			{
				parentName = i.ParentCategory.Name,
				parentId = i.ParentCategory.Id,
				childName = i.ChildCategory?.Name,
				childId = i.ChildCategory?.Id
			})
			.Select(i => i.Categories)
			.Where(c => c != null && c.Length > 0);

		public static IEnumerable<Ladder> GetCategoriesDistinct(this IEnumerable<Item> items)
			=> items
			?.Where(i => i.Categories != null)
			.SelectMany(l => l.Categories)
			.DistinctBy(l => new { l?.CategoryName, l?.CategoryId })
			.Where(l => !string.IsNullOrWhiteSpace(l?.CategoryName));
	}
}
