//// uncomment to run these tests
//#define L1_ENABLED

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using BaseLib;
using FluentAssertions;
using L1.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestCommon;

namespace ApiTests_L1
{
}

namespace ApiTests_L1.Inherited
{
#if !L1_ENABLED
	[Ignore]
#endif
	[TestClass]
	public class UserProfileAsync : ApiTests_L0.UserProfileAsync
	{
		public UserProfileAsync()
			=> api = InitializeState.REAL_InitAndGetApi().GetAwaiter().GetResult();
	}
}
