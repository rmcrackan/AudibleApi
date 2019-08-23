using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;

namespace Authentic.LoginCompleteTests
{
    [TestClass]
    public class ctor
    {
        [TestMethod]
        public void null_identity_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new LoginComplete(ApiClientMock.GetClient(), StaticSystemDateTime.Past, "x", null));
    }
}
