using NUnit.Framework;

namespace QuartersSDK.Data.Tests
{
    [TestFixture()]
    public class ResponseDataTests
    {
        [Test()]
        [Category("Success on ResponseData with OK response")]
        public void SuccessSetResponseDataTest()
        {
            string strJson = "{Balance: '200', access_token:'1234567', refresh_token:'1234-567', Scope:'mail'}";
            ResponseData response = new ResponseData(strJson, System.Net.HttpStatusCode.OK);

            Assert.That(response.IsSuccesful, Is.True);
            Assert.That(response.Balance, Is.EqualTo(200));
            Assert.That(response.AccessToken, Is.EqualTo("1234567"));
            Assert.That(response.RefreshToken, Is.EqualTo("1234-567"));
            Assert.That(response.Scope, Is.EqualTo("mail"));
        }

        [Test()]
        [Category("Success set error on ResponseData")]
        public void SuccessOnSetErrorResponseData()
        {
            ResponseData response = new ResponseData(new Error("{error:'Test error', error_description:'test description error'}"));

            Assert.IsFalse(response.IsSuccesful);
            Assert.IsNotNull(response.ErrorResponse);
        }

        [Test()]
        [Category("Success on set data on ResponseData")]
        public void SuccessOnSetDataTest()
        {
            string strJson = "{Balance: '200', access_token:'1234567', refresh_token:'1234-567', Scope:'mail'}";
            ResponseData response = new ResponseData();
            response.SetData(strJson, System.Net.HttpStatusCode.OK);

            Assert.That(response.IsSuccesful, Is.True);
            Assert.That(response.Balance, Is.EqualTo(200));
            Assert.That(response.AccessToken, Is.EqualTo("1234567"));
            Assert.That(response.RefreshToken, Is.EqualTo("1234-567"));
            Assert.That(response.Scope, Is.EqualTo("mail"));
        }
    }
}