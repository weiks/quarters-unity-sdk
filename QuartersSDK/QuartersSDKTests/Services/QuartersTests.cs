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
        private PCKE pcke;
        private APIClient client;
        private ILogger<Quarters> logger;

        [SetUp]
        public void Setup()
        {
            pcke = new PCKE();
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
        [Category("Fail on refresh not authorized token")]
        public void IsRefreshTokenRequestedFail()
        {
            Quarters q = new Quarters(client, logger);
            var res = q.GetRefreshToken("NO_AUTHORIZED_TOKEN");

            Assert.IsFalse(q._session.DoesHaveAccessToken);
            Assert.IsFalse(res.IsSuccesful);

            Assert.IsTrue(res.ErrorResponse.ErrorMessage.Equals("invalid_grant"));
            Assert.IsTrue(res.ErrorResponse.ErrorDescription.Equals("Invalid `code`"));
        }


        [Test]
        [Category("Success on refresh token")]
        public void IsRefreshTokenRequestedSuccess()
        {
            Quarters q = new Quarters(client, logger);
            string url = q.GetAuthorizeURL();
            //IMPORTANT: PASTE URL (url) ON INTERNET EXPLORER => Authorize => PASTE CODE AS PARAMETER HERE
            var res = q.GetRefreshToken("bRT1ffhmFKsKonwkbmZn2YBKxv9RtY_h");

            Assert.IsTrue(q._session.DoesHaveAccessToken);
            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);
        }

        [Test]
        [Category("Success on access token request")]
        public void IsAccessTokenRequestedSuccess()
        {
            Quarters q = new Quarters(client, logger);
            string url = q.GetAuthorizeURL();
            //IMPORTANT: PASTE URL (url) ON INTERNET EXPLORER => Authorize => PASTE CODE AS PARAMETER HERE
            q.GetRefreshToken("oQiyx6y9tLPXu31tQRde8__aZzW_uBtJ");
            
            var res = q.GetAccessToken();

            Assert.IsTrue(q._session.DoesHaveAccessToken);
            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);
        }

        [Test]
        [Category("Fail on refresh not authorized token")]
        public void IsAccessTokenRequestedFail()
        {
            Quarters q = new Quarters(client, logger);
            q.GetAccessToken();
            Assert.IsFalse(q._session.DoesHaveAccessToken);
        }

        [Test()]
        [Category("Success on get account balance request")]
        public void IsGetUserBalanceCallSuccess()
        {
            Quarters q = new Quarters(client, logger);
            string url = q.GetAuthorizeURL();
            //IMPORTANT: PASTE URL (url) ON INTERNET EXPLORER => Authorize => PASTE CODE AS PARAMETER HERE
            var res = q.GetRefreshToken("J91UOYaoWCPmgcZzQB2IsIm8yxr4Tjw6");

            var balance = q.GetAccountBalanceCall();

            Assert.IsTrue(q._session.DoesHaveAccessToken);
            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);

            Assert.Positive(balance);
        }


        [Test()]
        [Category("Success on get user details request")]
        public void IsGetUserDetailsCallSuccess()
        {
            Quarters q = new Quarters(client, logger);
            string url = q.GetAuthorizeURL();
            //IMPORTANT: PASTE URL (url) ON INTERNET EXPLORER => Authorize => PASTE CODE AS PARAMETER HERE
            var res = q.GetRefreshToken("yCkaNGenHQaTIFT2odQdjjGUujeCVrPU");

            var user = q.GetUserDetailsCall();

            Assert.IsTrue(q._session.DoesHaveAccessToken);
            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);
            
            Assert.IsFalse(string.IsNullOrEmpty(user.Id));
            Assert.IsFalse(string.IsNullOrEmpty(user.Email));
        }

        [Test()]
        public void MakeTransactionTest()
        {
            Quarters q = new Quarters(client, logger);
            Assert.Fail();
        }
    }
}