using Microsoft.Extensions.Logging;
using QuartersSDK.Data;
using QuartersSDK.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace QuartersSDK.Interfaces
{
    public interface IAPIClient
    {
        public ResponseData RequestPost(string url, RequestData request);

        public string RequestGet(string url, string request);

        public ResponseData RequestPost(string url, string token, Dictionary<string, object> dic);

    }
}
