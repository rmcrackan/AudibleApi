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

namespace Authentic.ResultFactoryTests.MfaSelectionPageFactoryTests
{
    [TestClass]
    public class IsMatchAsync
    {
        [TestMethod]
        public async Task match()
        {
            var match = @"<body><form id='auth-select-device-form' /></body>";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(match),
                StatusCode = HttpStatusCode.OK
            };
            (await ResultFactory.MfaSelectionPage.IsMatchAsync(response)).Should().BeTrue();
        }

        [TestMethod]
        public async Task no_match()
        {
            var noMatch = @"<body><form id='zauth-select-device-form' /></body>";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(noMatch),
                StatusCode = HttpStatusCode.OK
            };
            (await ResultFactory.MfaSelectionPage.IsMatchAsync(response)).Should().BeFalse();
        }
    }
}
