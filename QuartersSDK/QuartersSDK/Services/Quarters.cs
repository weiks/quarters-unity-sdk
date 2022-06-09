using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuartersSDK.Data;
using QuartersSDK.Interfaces;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

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

        private void LoadConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
            _api = new APIParams(_configuration.GetSection("ApiParams"));
            _app = new AppParams(_configuration.GetSection("AppParams"));
        }

        public Quarters(IAPIClient apiClient, ILogger<Quarters> logger)
        {
            LoadConfiguration();
            _pcke = new PCKE();
            _session = new Session();
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //TODO Make a single method for GetAccessToken and GetRefreshToken
        public ResponseData GetAccessToken()
        {
            try
            {
                //Debug.Log("Get Access token");
                _logger.LogInformation("Get Access token");

                if (!_session.DoesHaveRefreshToken)
                {
                    _logger.LogError("Missing refresh token");
                    return new ResponseData(new Error("Missing token", "Missing refresh token on session"));
                }

                RequestData request = new RequestData(grantType: "refresh_token",
                                                       clientId: _app.APP_ID,
                                                       clientSecret: _app.APP_KEY,
                                                       refreshToken: _session.RefreshToken,
                                                       codeVerifier: _pcke.CodeVerifier);
                var response = _apiClient.RequestPost(_api.ApiTokenURL, request);

                if (!response.IsSuccesful)
                    return response;

                _logger.LogInformation("GetAccessToken result " + response.ToJSONString());
                _session.DoRefresh(response);
                return response;
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

                RequestData request = new RequestData(grantType: "authorization_code",
                                                       clientId: _app.APP_ID,
                                                       clientSecret: _app.APP_KEY,
                                                       redirectUri: _app.REDIRECT_URL,
                                                       codeVerifier: _pcke.CodeVerifier,
                                                       code: code);
                var response = _apiClient.RequestPost(_api.ApiTokenURL, request);

                if (!response.IsSuccesful)
                    return response;

                _logger.LogInformation("GetRefreshToken result " + response.ToJSONString());
                _session.DoRefresh(response);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new ResponseData(new Error(ex.Message, ex.InnerException.ToString()));
            }
        }

        #region TODO UNITY Methods

        public IEnumerator GetAvatar(Action<Texture> OnSuccess, Action<Error> OnError)
        {
            throw new NotImplementedException();
        }

        private IEnumerator GetUserDetailsCall(Action<User> OnSuccess, Action<string> OnFailed, bool isRetry = false)
        {
            throw new NotImplementedException();
        }

        private IEnumerator GetAccountBalance(Action<long> OnSuccess, Action<string> OnFailed, bool isRetry = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerator MakeTransaction(long coinsQuantity, string description, Action OnSuccess, Action<string> OnFailed)
        {
            throw new NotImplementedException();
        }
        #endregion 
    }
}