﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Moq;
using NUnit.Framework;
using QuartersSDK.Data;
using QuartersSDK.Interfaces;
using System;

namespace QuartersSDK.Services.Tests
{
    [TestFixture()]
    public class QuartersTests
    {
        private PCKE pcke;
        private APIClient client;
        private ILogger<Quarters> logger;

        //Mocks
        private Mock<IQuarters> mockQuarters;

        [SetUp]
        public void Setup()
        {
            pcke = new PCKE();
            var serviceProvider = new ServiceCollection()
                                .AddLogging()
                                .BuildServiceProvider();

            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()
                                                    .AddFilter<EventLogLoggerProvider>((Func<LogLevel, bool>)(level => level >= LogLevel.Information))
                                                );
            logger = loggerFactory.CreateLogger<Quarters>();
            client = new APIClient(null);

            // Mocks & fakes data
            ResponseData fakeSuccessResponseAuthorized = new ResponseData("{id: '01xXXXXXX', Balance: '200', access_token:'u_IK3YYS95r9jo5TCME9h2h2NI3wypR2A69U3-Q9', refresh_token:'1234-567', Scope:'mail'}", System.Net.HttpStatusCode.OK);
            ResponseData fakeFailResponseTooMuchQuarters = new ResponseData(new Error("{error:'not_enough_quarters', error_description:'The address to debit does not have enough Quarters', status_code:'BadRequest'}"));
            User fakeUser = new User();
            fakeUser.Email = "mockuser@poq.gg";
            fakeUser.GamerTag = "mockuser";
            fakeUser.Id = "31415666";

            mockQuarters = new Mock<IQuarters>();
            mockQuarters.Setup(mk => mk.GetRefreshToken("mockCode")).Returns(fakeSuccessResponseAuthorized);
            mockQuarters.Setup(mk => mk.GetAccessToken()).Returns(fakeSuccessResponseAuthorized);
            mockQuarters.Setup(mk => mk.GetBuyQuartersUrl()).Returns("https://mocked.url");
            mockQuarters.Setup(mk => mk.GetAuthorizeUrl()).Returns("https://mocked.url");
            mockQuarters.Setup(mk => mk.GetAccountBalanceCall()).Returns(200);
            mockQuarters.Setup(mk => mk.GetUserDetailsCall()).Returns(fakeUser);
            mockQuarters.Setup(mk => mk.MakeTransaction(10, "SDK mock Test receive")).Returns(fakeSuccessResponseAuthorized);
            mockQuarters.Setup(mk => mk.MakeTransaction(-10, "SDK mock Test send")).Returns(fakeSuccessResponseAuthorized);
            mockQuarters.Setup(mk => mk.MakeTransaction(-10000, "SDK mock Test send too much")).Returns(fakeFailResponseTooMuchQuarters);
        }


        [Test]
        public void IsQuartersInstantiatingWithoutLogger()
        {
            PCKE pCKE = new PCKE();
            Quarters q = new Quarters(client, pcke.CodeChallenge(), pcke.CodeVerifier, "");
            Assert.IsTrue(!String.IsNullOrEmpty(q._api.BuyURL));
        }

        [Test]
        public void HasQuartersAPIParams()
        {
            PCKE pCKE = new PCKE();
            Quarters q = new Quarters(client, pcke.CodeChallenge(), pcke.CodeVerifier, "");
            Assert.IsTrue(!String.IsNullOrEmpty(q._api.BuyURL));
        }

        [Test]
        [Category("Fail on refresh not authorized token")]
        public void IsRefreshTokenRequestedFail()
        {
            Quarters q = new Quarters(client, logger, pcke.CodeChallenge(), pcke.CodeVerifier, "");
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
            string url = mockQuarters.Object.GetAuthorizeUrl();
            var res = mockQuarters.Object.GetRefreshToken("mockCode");

            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);
        }

        [Test]
        [Category("Success on access token request")]
        public void IsAccessTokenRequestedSuccess()
        {
            string url = mockQuarters.Object.GetAuthorizeUrl();
            mockQuarters.Object.GetRefreshToken("mockCode");

            var res = mockQuarters.Object.GetAccessToken();

            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);
            Assert.IsFalse(string.IsNullOrEmpty(res.AccessToken));
        }

        [Test]
        [Category("Fail on refresh not authorized token")]
        public void IsAccessTokenRequestedFail()
        {
            Quarters q = new Quarters(client, logger, pcke.CodeChallenge(), pcke.CodeVerifier, "");
            q.GetAccessToken();
            Assert.IsFalse(q._session.DoesHaveAccessToken);
        }

        [Test()]
        [Category("Success on get account balance request")]
        public void IsGetUserBalanceCallSuccess()
        {
            var res = mockQuarters.Object.GetRefreshToken("mockCode");

            var balance = mockQuarters.Object.GetAccountBalanceCall();

            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);
            Assert.IsFalse(string.IsNullOrEmpty(res.AccessToken));

            Assert.Positive(balance);
        }

        [Test()]
        [Category("Success on get user details request")]
        public void IsGetUserDetailsCallSuccess()
        {
            var res = mockQuarters.Object.GetRefreshToken("mockCode");
            var user = mockQuarters.Object.GetUserDetailsCall();

            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);

            Assert.IsFalse(string.IsNullOrEmpty(user.Id));
            Assert.IsFalse(string.IsNullOrEmpty(user.Email));
        }

        [Test()]
        [Category("Success on receive quarters transaction (possitive ammount) request")]
        public void IsReceiveTransactionTest()
        {
            string url = mockQuarters.Object.GetAuthorizeUrl();
            var res = mockQuarters.Object.GetRefreshToken("mockCode");
            var idTransaction = mockQuarters.Object.MakeTransaction(10, "SDK mock Test receive").IdTransaction;

            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);
            Assert.IsFalse(string.IsNullOrEmpty(idTransaction));
        }

        [Test()]
        [Category("Success on send quarters transaction (negative ammount) request")]
        public void IsSendTransactionTest()
        {
            string url = mockQuarters.Object.GetAuthorizeUrl();
            var res = mockQuarters.Object.GetRefreshToken("mockCode");
            var idTransaction = mockQuarters.Object.MakeTransaction(-10, "SDK mock Test send").IdTransaction;

            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(res.ErrorResponse == null);
            Assert.IsFalse(string.IsNullOrEmpty(idTransaction));
        }

        [Test()]
        [Category("Success on send too much quarters transaction (negative ammount) request")]
        public void IsSendTooMuchTransactionTest()
        {
            string url = mockQuarters.Object.GetAuthorizeUrl();
            var res = mockQuarters.Object.GetRefreshToken("mockCode");
            var transactionResponse = mockQuarters.Object.MakeTransaction(-10000, "SDK mock Test send too much");

            Assert.IsTrue(res.IsSuccesful);
            Assert.IsTrue(transactionResponse.ErrorResponse.StatusCode == System.Net.HttpStatusCode.BadRequest);
            Assert.IsTrue(transactionResponse.ErrorResponse.ErrorDescription.StartsWith("The address to debit does"));
            Assert.IsTrue(transactionResponse.ErrorResponse.ErrorMessage.Equals("not_enough_quarters"));
            Assert.IsTrue(string.IsNullOrEmpty(transactionResponse.IdTransaction));
            Assert.IsFalse(transactionResponse.IsSuccesful);
        }

        [Test()]
        [Category("Get Buy Quarters URL")]
        public void IsGetBuyQuartersUrlTest()
        {
            Quarters q = new Quarters(client, logger, pcke.CodeChallenge(), pcke.CodeVerifier, "");
            string buyUrl = q.GetBuyQuartersUrl();

            Assert.IsFalse(string.IsNullOrEmpty(buyUrl));
        }

        [Test()]
        [Category("Sign out and delete session")]
        public void IsSignedOut()
        {
            Quarters q = new Quarters(client, logger, pcke.CodeChallenge(), pcke.CodeVerifier, "");
            string url = q.GetAuthorizeUrl();
            //IMPORTANT: PASTE URL (url) ON INTERNET EXPLORER => Authorize => PASTE CODE AS PARAMETER HERE
            var res = q.GetRefreshToken("6hJHrZxFkZIGtOnHiRhqYkVTzK8t-qbH");

            q.SignOut();

            Assert.IsTrue(string.IsNullOrEmpty(q._session.AccessToken) && string.IsNullOrEmpty(q._session.RefreshToken));
        }
    }
}