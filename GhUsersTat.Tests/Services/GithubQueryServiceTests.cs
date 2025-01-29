using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using GhUsersTat.Services;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
using Moq.Protected;

namespace GhUsersTat.Tests
{
    [TestClass]
    public class GithubQueryServiceTests
    {
        private const string _validDataUsername = "octocat";
        private static string _maxReposCount;
        private static HttpResponseMessage _validUserResponse;
        private static HttpResponseMessage _validRepoResponse;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            _maxReposCount = ConfigurationManager.AppSettings["maxUserReposToReturn"];
            _validUserResponse = new HttpResponseMessage
            {
                Content = new StringContent(File.ReadAllText("TestData/ValidUserResponse.json")),
                StatusCode = HttpStatusCode.OK
            };
            _validRepoResponse = new HttpResponseMessage
            {
                Content = new StringContent(File.ReadAllText("TestData/ValidRepoResponse.json")),
                StatusCode = HttpStatusCode.OK
            };
        }

        private GithubQueryService GetServiceWithValidConfiguration(
            out Mock<HttpMessageHandler> httpHandler,
            out Mock<IMemoryCache> cache,
            out Mock<ILogger<GithubQueryService>> logger)
        {
            httpHandler = new Mock<HttpMessageHandler>();

            httpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.RequestUri.AbsolutePath == $"/users/{_validDataUsername}"),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(_validUserResponse));

            httpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(msg =>
                        msg.RequestUri.AbsolutePath == $"/users/{_validDataUsername}/repos"),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(_validRepoResponse));

            httpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                     ItExpr.Is<HttpRequestMessage>(msg =>
                        !msg.RequestUri.AbsolutePath.StartsWith($"/users/{_validDataUsername}")),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound }));

            cache = new Mock<IMemoryCache>();

            cache
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            logger = new Mock<ILogger<GithubQueryService>>();

            return new GithubQueryService(
                new HttpClient(httpHandler.Object),
                logger.Object,
                cache.Object);
        }

        [TestMethod]
        public async Task GetUserAsync_ValidUsername_CacheAndReturnUser()
        {
            var service = GetServiceWithValidConfiguration(out _, out var cache, out var logger);

            var user = await service.GetUserAsync(_validDataUsername);

            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Username);
            Assert.IsNotNull(user.Location);
            Assert.IsNotNull(user.AvatarUrl);
            Assert.IsNotNull(user.TopRepos);
            Assert.IsTrue(user.TopRepos.Count.ToString() == _maxReposCount);
            Assert.IsTrue(user.TopRepos[0].StarGazers >= user.TopRepos[2].StarGazers);

            cache.Verify(
                x => x.CreateEntry(It.IsAny<string>()),
                Times.Once(),
                "Cache was not set.");

            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, type) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeast(2),
                "Logger not called.");
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("username that doesn't match the valid username")]
        public async Task GetUserAsync_InvalidUsername_LogAndReturnNull(string username)
        {
            var service = GetServiceWithValidConfiguration(out _, out var cache, out var logger);

            var user = await service.GetUserAsync(username);

            Assert.IsNull(user);

            cache.Verify(
                x => x.CreateEntry(It.IsAny<string>()),
                Times.Never(),
                "Cache was set for an invalid request.");

            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, type) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeast(1),
                "Logger not called.");
        }
    }
}
