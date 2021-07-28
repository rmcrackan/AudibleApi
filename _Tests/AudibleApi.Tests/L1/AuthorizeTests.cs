//// uncomment to run these tests
//#define L1_ENABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using L1.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using TestAudibleApiCommon;
using TestCommon;
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authorize_L1
{
#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class AuthorizeTests
	{
		/// <summary>
		/// This method registers the test lib as an amazon device using tokens from
		/// your AccountsSettings.json copied into the test bin folder. Thic can if 
		/// the auth tokens from your initial login have not expired (6-hours from 
		/// the time Libation logged in with username and password). Once the test
		/// has been registered, all authorization data will be copied into the
		/// test bin folder and this method should not be called again.
		/// </summary>
		/// <returns></returns>
		[TestMethod]		
		public async Task<string> GetRegisterStringAsync()
		{
			var locale = Localization.Get("us");
			var auth = new Authorize(locale);
			var identity = REAL.GetIdentity();
			var regStr = await auth.RegisterAsync(identity.ExistingAccessToken, identity.Cookies.ToKeyValuePair());
			return regStr.ToString(Formatting.Indented);
		}
	}
}
