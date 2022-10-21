using QuartersSDK.Data;
using System.Collections.Generic;

namespace QuartersSDK.Interfaces
{
    public interface IAPIClient
    {
        ResponseData RequestPost(string url, RequestData request);

        string RequestGet(string url, string request);

        ResponseData RequestPost(string url, string token, Dictionary<string, object> dic);
    }
}