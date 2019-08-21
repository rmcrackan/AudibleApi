using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BaseLib
{
	public static class EnumExtensions
	{
		// https://stackoverflow.com/a/22132996
		public static IEnumerable<T> ToValues<T>(this T flags) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
				throw new ArgumentException("T must be an enumerated type.");

			var inputInt = (int)(object)flags;
			foreach (T value in Enum.GetValues(typeof(T)))
			{
				var valueInt = (int)(object)value;
				if (0 != (valueInt & inputInt))
				{
					yield return value;
				}
			}
		}

		// https://stackoverflow.com/a/30174850
		public static string GetDescription<T>(this T e) where T : struct, IConvertible
		{
			if (!(e is Enum))
				return null;

			var attribute =
				e.GetType()
				.GetTypeInfo()
				.GetMember(e.ToString())
				.FirstOrDefault(member => member.MemberType == MemberTypes.Field)
				?.GetCustomAttributes(typeof(DescriptionAttribute), false)
				.SingleOrDefault()
				as DescriptionAttribute;

			return attribute?.Description;
		}
	}
}
