using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuartersSDK.Data;
using QuartersSDK.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace QuartersSDK.Services
{
    public class APIClient : IAPIClient
    {
        public ILogger<Quarters> _logger;

        public APIClient(ILogger<Quarters> logger)
        {
            _logger = logger;
        }

        private static Dictionary<string, string> GetRequestContent(RequestData request)
        {
            try
            {
                var data = new Dictionary<string, string>();
                data.Add("grant_type", request.GrantType);
                data.Add("client_id", request.ClientId);
                data.Add("redirect_uri", request.RedirectUri);

                if (!String.IsNullOrEmpty(request.RefreshToken))
                    data.Add("refresh_token", request.RefreshToken);
                if (!String.IsNullOrEmpty(request.CodeVerifier))
                    data.Add("code_verifier", request.CodeVerifier);
                if (!String.IsNullOrEmpty(request.Code))
                    data.Add("code", request.Code);
                else
                    data.Add("client_secret", request.ClientSecret);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private ResponseData DoPost(HttpContent payload, string subPath, string token)
        {
            var rdo = new ResponseData();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(subPath);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(payload.Headers.ContentType.MediaType));

                    // make request
                    if (!string.IsNullOrEmpty(token))
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var request = httpClient.PostAsync(subPath, payload);
                    request.Wait();
                    var response = request.Result.Content.ReadAsStringAsync();

                    // check for error
                    if (!request.Result.IsSuccessStatusCode)
                    {
                        rdo.SetError(new Error(response.Result), request.Result.StatusCode);
                        return rdo;
                    }
                    rdo.SetData(response.Result, request.Result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"RequestPost : {ex.Message}| Stack: {ex.StackTrace} | InnerException: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
            return rdo;
        }

        public ResponseData RequestPost(string url, RequestData request)
        {
            try
            {
                _logger.LogInformation("RequestPost : ");
                var data = new FormUrlEncodedContent(GetRequestContent(request));
                return DoPost(data, url, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RequestPost : {ex.Message}| Stack: {ex.StackTrace} | InnerException: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        public ResponseData RequestPost(string url, string token, Dictionary<string, object> dic)
        {
            try
            {
                _logger.LogInformation("RequestPost : ");
                var jsonString = JsonConvert.SerializeObject(dic);
                var data = new StringContent(jsonString, Encoding.UTF8, "application/json");
                return DoPost(data, url, token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RequestPost dic : {ex.Message}| Stack: {ex.StackTrace} | InnerException: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        public ResponseData RequestPostMultipartForm(string url, RequestData request)
        {
            var rdo = new ResponseData();
            try
            {
                _logger.LogInformation("RequestPostMultipartForm : ");
                Dictionary<string, string> data = GetRequestContent(request);
                using (var httpClient = new HttpClient())
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(data) };
                    var res = httpClient.SendAsync(req);
                    res.Wait();
                    var response = res.Result.Content.ReadAsStringAsync();
                    rdo.SetData(response.Result, res.Result.StatusCode);
                }
                return rdo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"RequestPostMultipartForm : {ex.Message}| Stack: {ex.StackTrace} | InnerException: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        public string RequestGet(string url, string requestToken)
        {
            try
            {
                string responseString = string.Empty;

                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                _logger.LogInformation($"RequestGet : | timeout: {httpRequest.Timeout}");

                httpRequest.Accept = "application/json";
                httpRequest.Headers["Authorization"] = $"Bearer {requestToken}";

                using (var response = (HttpWebResponse)httpRequest.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseString = reader.ReadToEnd();
                    }
                }
                return responseString;
            }
            catch (Exception ex)
            {
                _logger.LogError($"RequestGet : {ex.Message}| Stack: {ex.StackTrace} | InnerException: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }
    }
}