using NUnit.Framework;
using QuartersSDK.Data.Enums;
using System;

namespace QuartersSDK.Data.Tests
{
    [TestFixture()]
    public class RequestDataTests
    {
        [Test()]
        [Category("Error on No ClientId on RequestData")]
        public void IsErrorNoClientIdRequestDataTest()
        {
            RequestData request;
            var ex = Assert.Throws<ArgumentNullException>(() => request = new RequestData(grantType: "refresh_token",
                                                       clientId: String.Empty,
                                                       refreshToken: "",
                                                       codeVerifier: ""));
            Assert.That(ex.ParamName, Is.EqualTo("clientId"));
        }

        [Test()]
        [Category("Success Authorization RequestData")]
        public void IsSuccessAuthorizationRequestDataTest()
        {
            RequestData request = new RequestData(grantType: EnumUtils.ToEnumString(GrantType.AUTHORIZATION_CODE),
                                                       clientId: "314",
                                                       refreshToken: "",
                                                       codeVerifier: "");
            Assert.That(request.GrantType, Is.EqualTo("authorization_code"));
        }

        [Test()]
        [Category("Success Refresh Token RequestData")]
        public void IsSuccessRefreshTokenRequestDataTest()
        {
            RequestData request = new RequestData(grantType: EnumUtils.ToEnumString(GrantType.REFRESH_TOKEN),
                                                       clientId: "314",
                                                       refreshToken: "",
                                                       codeVerifier: "");
            Assert.That(request.GrantType, Is.EqualTo("refresh_token"));
        }
    }
}