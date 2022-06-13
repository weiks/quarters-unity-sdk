using NUnit.Framework;
using QuartersSDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuartersSDK.Data.Tests
{
    [TestFixture()]
    public class ErrorTests
    {
        [Test()]
        [Category("Creates missing token error")]
        public void InstantiatesErrorTest()
        {
           Error err = new Error("Missing token", "Missing refresh token on session");

            Assert.That(err.ErrorMessage.Equals("Missing token"));
            Assert.That(err.ErrorDescription.Equals("Missing refresh token on session"));
        }

        [Test()]
        [Category("Creates error with JSON")]
        public void InstantiatesWithJSONTest()
        {
            Error err = new Error("{error:'Test error', error_description:'test description error'}");

            Assert.That(err.ErrorMessage.Equals("Test error"));
            Assert.That(err.ErrorDescription.Equals("test description error"));
        }

    }
}