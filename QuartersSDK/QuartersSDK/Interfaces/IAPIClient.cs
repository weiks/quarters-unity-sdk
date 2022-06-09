using QuartersSDK.Data;
using QuartersSDK.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace QuartersSDK.Interfaces
{
    public interface IAPIClient
    {
        public ResponseData RequestPost(string url, RequestData request);
        public void BuyQuarters() { }
    }
}
