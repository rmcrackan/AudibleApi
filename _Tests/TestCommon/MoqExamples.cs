using System;
using System.Threading;
using System.Threading.Tasks;
using BaseLib;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace MoqExamples
{
    [TestClass]
    public class scratchPad
    {
        public interface IRandomNumberGenerator
        {
            int Next();
            int Next(int minValue, int maxValue);
        }

        [TestMethod]
        public void constant_return_method()
        {
            var moq1 = new Mock<IRandomNumberGenerator>();
            moq1.Setup(a => a.Next()).Returns(3);
            moq1.Object.Next().Should().Be(3);
        }

        [TestMethod]
        public void constant_return_get()
        {
            var dt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var moq1 = new Mock<ISystemDateTime>();
            moq1.SetupGet(a => a.UtcNow).Returns(dt);
            moq1.Object.UtcNow.Should().Be(dt);
        }

        [TestMethod]
        public void conditional_return()
        {
            var moq = new Mock<IRandomNumberGenerator>();
            moq.Setup(z => z.Next(995, 1005)).Returns(1001);
            var rng = moq.Object;

            var undefined = rng.Next(1, 2);
            undefined.Should().Be(0);

            var _1001 = rng.Next(995, 1005);
            _1001.Should().Be(1001);
        }

        [TestMethod]
        public void return_sequentially()
        {
            var _mockClient = new Mock<ISystemDateTime>();
            var dt1st = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var dt2nd = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _mockClient.SetupSequence(m => m.UtcNow)
                // 1st call to Now returns this value
                .Returns(dt1st)
                // 2nd call returns this value
                .Returns(dt2nd);
            var obj = _mockClient.Object;
            obj.UtcNow.Should().Be(dt1st);
            obj.UtcNow.Should().Be(dt2nd);
        }

        [TestMethod]
        public async Task complex_sequence()
        {
            // ARRANGE mock
            var handlerMock = new Mock<System.Net.Http.HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup("Dispose", ItExpr.IsAny<bool>())
                .Verifiable();

            handlerMock
                .Protected()
                .SetupSequence<Task<System.Net.Http.HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<System.Net.Http.HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // 1st handler call: throws
                .ThrowsAsync(new TimeoutException())
                //  2nd handler call: passes
                .ReturnsAsync(new System.Net.Http.HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Content = new System.Net.Http.StringContent("foo bar")
                })
                //  3rd handler call: throws
                .ThrowsAsync(new ArgumentException())
                ;

            var client = new System.Net.Http.HttpClient(handlerMock.Object);
            async Task<System.Net.Http.HttpResponseMessage> callClient()
                => await client.SendAsync(
                    new System.Net.Http.HttpRequestMessage(
                        System.Net.Http.HttpMethod.Get,
                        new Uri("http://test.com")
                        )
                    );

            // ACT and ASSERT

            // 1st call
            await Assert.ThrowsExceptionAsync<TimeoutException>(() => callClient());

            // 2nd call
            var response = await callClient();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.ServiceUnavailable);
            (await response.Content.ReadAsStringAsync()).Should().Be("foo bar");

            // 3rd call
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => callClient());
        }
    }
}
