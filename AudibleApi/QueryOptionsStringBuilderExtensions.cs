using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudibleApi
{
	internal static class QueryOptionsStringBuilderExtensions
	{
		internal static string ToResponseGroupsQueryString(this Enum responseGroupOptions)
			=> "response_groups=" + responseGroupOptions.ToFlagDescriptions();
		internal static string ToImageSizesQueryString(this Enum responseGroupOptions)
			=> "image_sizes=" + responseGroupOptions.ToFlagDescriptions();
		internal static string ToSortByQueryString(this Enum responseGroupOptions)
			=> "sort_by=" + responseGroupOptions.ToDescription();

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
