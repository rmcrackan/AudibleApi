using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudibleApi
{
	public static class QueryOptionsStringBuilderExtensions
	{
		public static string ToResponseGroupsQueryString(this Enum responseGroupOptions)
		{
			var respondeGroupes = responseGroupOptions.ToFlagDescriptions();
			if (string.IsNullOrEmpty(respondeGroupes))
				return string.Empty;
			return "response_groups=" + respondeGroupes;
		}
		public static string ToImageSizesQueryString(this Enum imageSizeOptions)
		{
			var imageSizes = imageSizeOptions.ToFlagDescriptions();
			if (string.IsNullOrEmpty(imageSizes))
				return string.Empty;
			return "image_sizes=" + imageSizes;
		}
		public static string ToSortByQueryString(this Enum sortByOptions)
		{
			var sortBy = sortByOptions.ToDescription();
			if (string.IsNullOrEmpty(sortBy))
				return string.Empty;
			return "sort_by=" + sortBy;
		}

		private static string ToFlagDescriptions(this Enum flagEnumValue)
		{
			if (!flagEnumValue.GetType().IsDefined(typeof(FlagsAttribute), false))
				throw new ArgumentException($"This method only supports enum types with Flags attribute.");

			if (Convert.ToUInt64(flagEnumValue) == 0)
				return "";

			var descriptions = flagEnumValue.getFlaggedDescriptions().ToList();

			if (!descriptions.Any() || descriptions.Any(d => d is null))
				throw new Exception("Unexpected value in response group");
			return descriptions.Aggregate((a, b) => $"{a},{b}");
		}

		private static string ToDescription(this Enum enumValue)
		{
			if (enumValue.GetType().IsDefined(typeof(FlagsAttribute), false))
				throw new ArgumentException($"This method does no support enum types with Flags attribute.");

			if (Convert.ToUInt64(enumValue) == 0)
				return "";

			var description = enumValue.GetDescription();
			if (description is null)
				throw new Exception("Unexpected value for sort by");
			return description;
		}

		private static IEnumerable<string> getFlaggedDescriptions(this Enum input)
		{
			Type enumType = input.GetType();
			if (!enumType.IsEnum)
				yield break;

			ulong setBits = Convert.ToUInt64(input);

			// check each enum value mask if it is in input bits
			foreach (Enum value in Enum.GetValues(enumType))
			{
				ulong valMask = Convert.ToUInt64(value);

				if ((setBits & valMask) == valMask && isPowerOfTwo(valMask))
					yield return value.GetDescription();
			}
		}

		static bool isPowerOfTwo(ulong n)
		{
			if (n == 0)
				return false;

			while (n != 1)
			{
				if (n % 2 != 0)
					return false;

				n /= 2;
			}
			return true;
		}
	}
}
