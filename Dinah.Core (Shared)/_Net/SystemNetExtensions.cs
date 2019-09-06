using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Dinah.Core.Net
{
	public static class SystemNetExtensions
	{
		public static IEnumerable<Cookie> EnumerateCookies(this CookieContainer cookieJar, Uri uri) => cookieJar.GetCookies(uri).Cast<Cookie>();

		// https://stackoverflow.com/a/14074200
		public static Hashtable ReflectOverAllCookies(this CookieContainer cookies)
			=> (Hashtable)cookies.GetType().InvokeMember(
				"m_domainTable",
				BindingFlags.NonPublic |
				BindingFlags.GetField |
				BindingFlags.Instance,
				null,
				cookies,
				new object[] { });
	}
}
