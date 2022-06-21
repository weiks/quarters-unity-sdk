using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuartersSDK.Data;
using QuartersSDK.Data.Enums;
using QuartersSDK.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

namespace QuartersSDK.Services
{
    public class Quarters : IQuarters
    {
        public PCKE _pcke;
        public Session _session;
        public AppParams _app;
        public APIParams _api;
        public IAPIClient _apiClient; 
        public ILogger<Quarters> _logger;
        private IConfiguration _configuration;

        public Quarters(IAPIClient apiClient, ILogger<Quarters> logger)
        {
            LoadConfiguration();
            _pcke = new PCKE();
            _session = new Session();
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void LoadConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
            _api = new APIParams(_configuration.GetSection("ApiParams"));
            _app = new AppParams(_configuration.GetSection("AppParams"));
        }
        
        private string GetStrResponse(string url, string token)
        {
            var response = _apiClient.RequestGet(url, token);
            var sr = new StreamReader(response.GetResponseStream());
            var strResponse = sr.ReadToEnd();
            if (!new HttpResponseMessage(response.StatusCode).IsSuccessStatusCode)
            {
                _logger.LogError(strResponse);
                _logger.LogError(response.StatusCode.ToString(), response.StatusDescription);
                throw new Error(response.StatusCode.ToString(), response.StatusDescription);
            }

            _logger.LogInformation($"url: {url} | response: {strResponse}");
            return strResponse;
        }

        private ResponseData RequestAuthorize(RequestData request)
        {
            var response = _apiClient.RequestPost(_api.ApiTokenURL, request);

            if (!response.IsSuccesful)
                return response;

            _logger.LogInformation($"{_api.ApiTokenURL} result {response.ToJSONString()}");
            _session.DoRefresh(response);
            return response;
        }

        public string GetAuthorizeURL()
        {
            return $"{_api.ApiAuthorizeURL}?response_type=code&" +
                $"client_id={_app.APP_ID}&" +
                $"redirect_uri={_app.REDIRECT_URL}&" +
                $"scope=identity+email+transactions+events+wallet&" +
                $"code_challenge_method=S256&" +
                $"code_challenge={_pcke.CodeChallenge()}";
        }

        #region UNITY Methods
        public ResponseData GetAccessToken()
        {
            try
            {
                _logger.LogInformation("Get Access token");

                if (!_session.DoesHaveRefreshToken)
                {
                    _logger.LogError("Missing refresh token");
                    return new ResponseData(new Error("Missing token", "Missing refresh token on session"));
                }

                RequestData request = new RequestData(grantType: EnumUtils.ToEnumString(GrantType.REFRESH_TOKEN),
                                                       clientId: _app.APP_ID,
                                                       clientSecret: _app.APP_KEY,
                                                       refreshToken: _session.RefreshToken,
                                                       codeVerifier: _pcke.CodeVerifier);
                return RequestAuthorize(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ResponseData(new Error(ex.Message, ex.InnerException.ToString()));
            }
        }

        public ResponseData GetRefreshToken(string code)
        {
            try
            {
                _logger.LogInformation($"Get refresh token with code: {code}");

                RequestData request = new RequestData(grantType: EnumUtils.ToEnumString(GrantType.AUTHORIZATION_CODE),
                                                       clientId: _app.APP_ID,
                                                       clientSecret: _app.APP_KEY,
                                                       redirectUri: _app.REDIRECT_URL,
                                                       codeVerifier: _pcke.CodeVerifier,
                                                       code: code);
                return RequestAuthorize(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ResponseData(new Error(ex.Message, ex.InnerException.ToString()));
            }
        }

        public User GetUserDetailsCall()
        {
            _logger.LogInformation($"GetUserDetailsCall - URL:{_api.UserDetailsURL}");
            try
            {
                var responseAccessToken = new ResponseData();
                if (!_session.DoesHaveAccessToken)
                    responseAccessToken = GetAccessToken();

                string strResponse = GetStrResponse(_api.UserDetailsURL, _session.AccessToken);
                return JsonConvert.DeserializeObject<User>(strResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        public long GetAccountBalanceCall()
        {
            _logger.LogInformation($"GetAccountBalanceCall - URL:{_api.BalanceURL}");
            try
            {
                string strResponse = GetStrResponse(_api.BalanceURL, _session.AccessToken);
                JObject responseData = JsonConvert.DeserializeObject<JObject>(strResponse);
                return responseData["balance"].ToObject<long>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        public bool MakeTransaction(long coinsQuantity, string description)
        {
            _logger.LogInformation($"MakeTransaction with quantity: {coinsQuantity}");

            try
            {
                if (!_session.DoesHaveRefreshToken)
                    _logger.LogError("Missing refresh token");

                _logger.LogInformation("Transaction url: " + _api.TransactionsURL);

                Dictionary<string, object> postData = new Dictionary<string, object>();
                postData.Add("creditUser", coinsQuantity);
                postData.Add("description", description);
                var response = _apiClient.RequestPost(url: _api.TransactionsURL, token: _session.AccessToken, dic: postData);
                if (!new HttpResponseMessage(response.StatusCode).IsSuccessStatusCode)
                {
                    _logger.LogError(response.ToJSONString());
                    _logger.LogError($"Status: {response.StatusCode} |Response: {response.ToJSONString()}");

                    Error error = new Error(response.ToJSONString());

                    if (error.ErrorDescription == Error.INVALID_TOKEN) //dispose invalid refresh token
                        _session.RefreshToken = "";
                    return false;
                    // VspAttribution.VspAttribution.SendAttributionEvent("TransactionFailed", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                }

                _logger.LogInformation(response.ToJSONString());
                //VspAttribution.VspAttribution.SendAttributionEvent("TransactionSuccess", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }
        
        public Texture GetAvatar(User u)
        {
            _logger.LogInformation($"Pull avatar: {_api.AvatarURL(u)}");

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(_api.AvatarURL(u));

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                _logger.LogError(request.error);
                _logger.LogError(request.downloadHandler.text);

                throw new Error(request.downloadHandler.text);
            }

            return ((DownloadHandlerTexture)request.downloadHandler).texture;
        }

        #endregion

     

        #region TODO UNITY Methods
        public void BuyQuarters()
        {

        }
        #endregion 
    }
}