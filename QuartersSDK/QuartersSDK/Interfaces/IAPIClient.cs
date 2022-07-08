using QuartersSDK.Data;
using System.Collections.Generic;

namespace QuartersSDK.Interfaces
{
    public interface IAPIClient
    {
        public ResponseData RequestPost(string url, RequestData request);

        public string RequestGet(string url, string request);

        public ResponseData RequestPost(string url, string token, Dictionary<string, object> dic);
    }
}