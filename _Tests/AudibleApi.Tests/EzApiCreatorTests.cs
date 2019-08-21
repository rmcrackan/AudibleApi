using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using BaseLib;
using FluentAssertions;
using L1.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EzApiCreatorTests_L0
{
	[TestClass]
	public class GetApiAsync_string
	{
		[TestMethod]
		public async Task access_from_L0_throws()
			=> await Assert.ThrowsExceptionAsync<MethodAccessException>(() => EzApiCreator.GetApiAsync("file"));
	}

	[TestClass]
	public class GetApiAsync_string_identity
	{
		[TestMethod]
		public async Task access_from_L0_throws()
			=> await Assert.ThrowsExceptionAsync<MethodAccessException>(() => EzApiCreator.GetApiAsync("file", new Mock<IIdentity>().Object));
	}
}

namespace EzApiCreatorTests_L1
{
}
