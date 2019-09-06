using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// FROM
// https://lostechies.com/jimmybogard/2008/08/12/enumeration-classes/
// tarantino / src / Tarantino.DatabaseManager.Core / Enumeration.cs
//   https://bitbucket.org/toddwood/tarantino/src/47e07dee9dc880f75f7c5e710d41519ed9c716b4/src/Tarantino.DatabaseManager.Core/Enumeration.cs
namespace Dinah.Core
{
	[Serializable]
	public abstract class Enumeration : IComparable
	{
		public int Value { get; }
		public string DisplayName { get; }

		protected Enumeration(int value, string displayName)
		{
			Value = value;
			DisplayName = displayName;
		}

		public override string ToString() => DisplayName;

		private static Dictionary<Type, IEnumerable<Enumeration>> cache { get; } = new Dictionary<Type, IEnumerable<Enumeration>>();

		// better implementation of GetAll from comments of
		// https://lostechies.com/jimmybogard/2008/08/12/enumeration-classes/
		public static IEnumerable<T> GetAll<T>() where T : Enumeration
		{
			var t = typeof(T);

			// cache. reflection is too expensive to do naively
			if (!cache.ContainsKey(t))
			{
				var flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
				var properties = t
					.GetProperties(flags)
					.Select(p => p.GetValue(null))
					.Cast<T>()
					.ToList();
				var fields = typeof(T)
					.GetFields(flags)
					.Select(f => f.GetValue(null))
					.Cast<T>()
					.ToList();
				cache.Add(t, properties.Concat(fields));
			}

			return cache[t] as IEnumerable<T>;
		}

		//public static IEnumerable GetAll(Type type)
		//{
		//    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
		//    foreach (var info in properties)
		//        yield return info.GetValue(Activator.CreateInstance(type));
		//}

		public override bool Equals(object obj)
		{
			if (!(obj is Enumeration otherValue))
				return false;

			var typeMatches = GetType().Equals(obj.GetType());
			var valueMatches = Value.Equals(otherValue.Value);

			return typeMatches && valueMatches;
		}

		public override int GetHashCode() => Value.GetHashCode();

		public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue) => Math.Abs(firstValue.Value - secondValue.Value);

		public static T FromValue<T>(int value) where T : Enumeration
			=> parse<T, int>(value, "value", item => item.Value == value);

		public static T FromDisplayName<T>(string displayName) where T : Enumeration
			=> parse<T, string>(displayName, "display name", item => item.DisplayName == displayName);

		private static T parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration
			=> GetAll<T>().FirstOrDefault(predicate)
			?? throw new ApplicationException($"'{value}' is not a valid {description} in {typeof(T)}");

		public virtual int CompareTo(object obj) => Value.CompareTo(((Enumeration)obj).Value);
	}
}