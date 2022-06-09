using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using QuartersSDK.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuartersSDK.Services.Tests
{
    [TestFixture()]
    public class QuartersTests
    {
        private APIClient client;
        private ILogger<Quarters> logger;

        [SetUp]
        public void Setup()
        {
            client = new APIClient();

            var serviceProvider = new ServiceCollection()
                                .AddLogging()
                                .BuildServiceProvider();

            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            logger = loggerFactory.CreateLogger<Quarters>();
        }

        [Test]
        public void HasQuartersAPIParams()
        {
            Quarters q = new Quarters(client, logger);
            Assert.IsTrue(!String.IsNullOrEmpty(q._api.BuyURL));
        }

        [Test]
        public void IsAccessTokenRequestedFail()
        {
            Quarters q = new Quarters(client, logger);
            q.GetAccessToken();
            Assert.IsFalse(q._session.DoesHaveAccessToken);
        }

        [Test]
        public void IsRefreshTokenRequestedFail()
        {
            Quarters q = new Quarters(client, logger);
            q.GetRefreshToken("CodeToGetFromWebView");
            Assert.IsFalse(q._session.DoesHaveAccessToken);
        }

        [Test()]
        public void GetAvatarTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void MakeTransactionTest()
        {
            Assert.Fail();
        }
    }
}