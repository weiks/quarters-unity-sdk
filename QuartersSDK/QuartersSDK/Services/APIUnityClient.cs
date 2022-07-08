using NetTopologySuite.Utilities;
using Newtonsoft.Json;
using QuartersSDK.Data;
using QuartersSDK.Data.Enums;
using QuartersSDK.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;


namespace QuartersSDK.Services
{
    public class APIUnityClient : IAPIClient
    {
     
        private IEnumerator DoPost(string url)
        {
            throw new NotImplementedException();
        }

        public ResponseData RequestPost(string url, RequestData request)
        {
            throw new NotImplementedException();
        }

        public string RequestGet(string url, string request)
        {
            throw new NotImplementedException();
        }

        public ResponseData RequestPost(string url, string token, Dictionary<string, object> dic)
        {
            throw new NotImplementedException();
        }
    }
}
