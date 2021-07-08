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
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;

namespace Authentic.ResultFactoryTests
{
    public class ConcreteResultFactory : ResultFactory
    {
        public ConcreteResultFactory() : base(nameof(ConcreteResultFactory)) { }

        protected override async Task<bool> _isMatchAsync(HttpResponseMessage response)
            => await response.Content.ReadAsStringAsync() == "IsMatch";
    }

    [TestClass]
    public class IsMatchAsync
    {
        [TestMethod]
        public async Task null_response_returns_false()
            => (await new ConcreteResultFactory().IsMatchAsync(null)).Should().BeFalse();

        [TestMethod]
        public async Task null_content_returns_false()
            => (await new ConcreteResultFactory().IsMatchAsync(new HttpResponseMessage())).Should().BeFalse();

        [TestMethod]
        public async Task invalid_content_returns_false()
            => (await new ConcreteResultFactory().IsMatchAsync(new HttpResponseMessage { Content = new StringContent("x") })).Should().BeFalse();

        [TestMethod]
        public async Task valid_content_returns_true()
            => (await new ConcreteResultFactory().IsMatchAsync(new HttpResponseMessage { Content = new StringContent("IsMatch") })).Should().BeTrue();
    }

    [TestClass]
    public class CreateResultAsync
    {
        [TestMethod]
        public async Task null_authenticate_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => new ConcreteResultFactory().CreateResultAsync(null, new HttpResponseMessage { Content = new StringContent("x") }, new Dictionary<string, string>()));

        [TestMethod]
		public async Task null_response_throws()
			=> await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => new ConcreteResultFactory().CreateResultAsync(AuthenticateShared.GetAuthenticate(), null, new Dictionary<string, string>()));

        [TestMethod]
        public async Task null_inputs_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => new ConcreteResultFactory().CreateResultAsync(AuthenticateShared.GetAuthenticate(), new HttpResponseMessage { Content = new StringContent("x") }, null));

        [TestMethod]
        public async Task false_IsMatch_throws()
        {
            var badMatch = "x";
            await Assert.ThrowsExceptionAsync<LoginFailedException>(() => new ConcreteResultFactory().CreateResultAsync(AuthenticateShared.GetAuthenticate(), new HttpResponseMessage { Content = new StringContent(badMatch) }, new Dictionary<string, string>())
            );
        }

        [TestMethod]
        public async Task valid_returns_null()
        {
            var result = await new ConcreteResultFactory().CreateResultAsync(AuthenticateShared.GetAuthenticate(), new HttpResponseMessage { Content = new StringContent("IsMatch") }, new Dictionary<string, string>());
            result.Should().BeNull();
        }
    }
}
