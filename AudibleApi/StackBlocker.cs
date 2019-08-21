using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;

namespace AudibleApi
{
	public static class StackBlocker
	{
		public static Regex L1Regex { get; }
			= new Regex(@"(\.|_|^)L1(\.|_|$)",
				RegexOptions.IgnoreCase |
				RegexOptions.Compiled);

		/// <summary>
		/// Throw if any part of the call stack includes AudibleApi.Tests
		/// </summary>
		public static void ApiTestBlocker()
		{
			var stackTrace = new StackTrace(true);

			var frames = stackTrace
				.GetFrames()
				.ToList();

			var namespaces = frames
				.Select(f => f.GetMethod().ReflectedType.Namespace)
				.Distinct()
				.ToList();

			// L1 tests are allowed to use methods which rely on ApiTestBlocker() and the also often call L0
			var isL1 = namespaces
				.SelectMany(ns => ns.Split())
				.Any(s => L1Regex.IsMatch(s));
			if (isL1)
				return;

			var frames2 = frames
				.Select(f => f.GetMethod().ReflectedType.Assembly.GetName().Name)
				.Distinct()
				.ToList();
			if (frames2.Contains("AudibleApi.Tests"))
				throw new MethodAccessException("This is for production only. Do not call from L0 tests");
		}
	}
}
