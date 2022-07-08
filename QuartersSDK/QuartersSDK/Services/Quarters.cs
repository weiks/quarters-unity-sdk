using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuartersSDK.Data;
using QuartersSDK.Data.Enums;
using QuartersSDK.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

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
        public string CodeChallenge { get; set; }
        public string CodeVerifier { get; set; }

        public Quarters(IAPIClient apiClient, ILogger<Quarters> logger, string codeChallenge, string codeVerifier, string refreshToken)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            LoadConfiguration();
            _session = new Session();
            _session.RefreshToken = refreshToken;
            CodeChallenge = codeChallenge;
            CodeVerifier = codeVerifier;
        }

        public Quarters(IAPIClient apiClient, ILogger<Quarters> logger, string refreshToken)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            LoadConfiguration();
            _pcke = new PCKE();
            _session = new Session();
            _session.RefreshToken = refreshToken;
            CodeChallenge = _pcke.CodeChallenge();
            CodeVerifier = _pcke.CodeVerifier;
        }

        public Quarters(IAPIClient apiClient, ILogger<Quarters> logger, string refreshToken, Dictionary<string, string> appSettings, Dictionary<string, string> apiSettings)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (appSettings.Count == 0 || appSettings == null) throw new ArgumentNullException(nameof(appSettings));
            if (apiSettings.Count == 0 || apiSettings == null) throw new ArgumentNullException(nameof(apiSettings));

            _pcke = new PCKE();
            _session = new Session();
            _app = new AppParams(appSettings);
            _api = new APIParams(apiSettings);
            _session.RefreshToken = refreshToken;
            CodeChallenge = _pcke.CodeChallenge();
            CodeVerifier = _pcke.CodeVerifier;
        }

        private void LoadConfiguration()
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

                _configuration = builder.Build();
                _api = new APIParams(_configuration.GetSection("ApiParams"));
                _app = new AppParams(_configuration.GetSection("AppParams"));
            }
            catch (Exception ex)
            {
                Error error = new Error();
                _logger.LogError($"LoadConfiguration | Message: {ex.Message} | InnerException: {ex.InnerException.ToString()}");
                error.ErrorDescription = $"Directory: {Directory.GetCurrentDirectory()} |  Message: {ex.Message} | InnerException: {ex.InnerException.ToString()} ";
                throw error;
            }
        }

        private string GetStrResponse(string url, string token)
        {
            try
            {
                _logger.LogInformation($"GetStrResponse| url: {url} | token: {token}");
                var strResponse = _apiClient.RequestGet(url, token);
                _logger.LogInformation($"GetStrResponse| response: {strResponse}");

                if (string.IsNullOrEmpty(strResponse))
                    _logger.LogError("Quarters|GetStrResponse| strResponse: is null or empty ");

                return strResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetStrResponse | Message: {ex.Message} | StackTrace: {ex.StackTrace}");
                throw ex;
            }
        }

        private ResponseData RequestAuthorize(RequestData request)
        {
            try
            {
                _logger.LogInformation($"Quarters|RequestAuthorize");
                var response = _apiClient.RequestPost(_api.ApiTokenURL, request);

                if (!response.IsSuccesful)
                    return response;

                _session.DoRefresh(response);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"RequestAuthorize | Message: {ex.Message} | InnerException: {ex.InnerException.ToString()}");
                throw ex;
            }
        }

        public string GetAuthorizeUrl()
        {
            return $"{_api.ApiAuthorizeURL}?response_type=code&" +
                $"client_id={_app.APP_ID}&" +
                $"redirect_uri={_app.REDIRECT_URL}&" +
                $"scope=identity+email+transactions+events+wallet&" +
                $"code_challenge_method=S256&" +
                $"code_challenge={CodeChallenge}";
        }

        public string GetSchemaUrl()
        {
            return _app.SCHEMA_URL;
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

                RequestData request = new RequestData(grantType: "refresh_token",
                                                       clientId: _app.APP_ID,
                                                       clientSecret: _app.APP_KEY,
                                                       refreshToken: _session.RefreshToken,
                                                       codeVerifier: CodeVerifier);
                return RequestAuthorize(request);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error GetAccessToken: {ex.Message} | Stacktrace: {ex.StackTrace} | Desscription: {ex.InnerException}");
                return new ResponseData(new Error(ex.Message, ex.InnerException.ToString()));
            }
        }

        public ResponseData GetRefreshToken(string code)
        {
            try
            {
                _logger.LogInformation($"Get refresh token with code");

                RequestData request = new RequestData(grantType: EnumUtils.ToEnumString(GrantType.AUTHORIZATION_CODE),
                                                       clientId: _app.APP_ID,
                                                       clientSecret: _app.APP_KEY,
                                                       redirectUri: _app.REDIRECT_URL,
                                                       codeVerifier: CodeVerifier,
                                                       code: code);
                return RequestAuthorize(request);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error GetRefreshToken: {ex.Message} | Stacktrace: {ex.StackTrace} | Desscription: {ex.InnerException}");
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
                _logger.LogError($"Error GetUserDetailsCall: {ex.Message} | Stacktrace: {ex.StackTrace} | Desscription: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        public string GetUserDetailsCallStr()
        {
            _logger.LogInformation($"GetUserDetailsCall - URL:{_api.UserDetailsURL}");
            try
            {
                var responseAccessToken = new ResponseData();
                if (!_session.DoesHaveAccessToken)
                    responseAccessToken = GetAccessToken();

                return GetStrResponse(_api.UserDetailsURL, _session.AccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error GetUserDetailsCallStr: {ex.Message} | Stacktrace: {ex.StackTrace} | Desscription: {ex.InnerException}");
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
                _logger.LogError($"Error GetAccountBalanceCall: {ex.Message} | Stacktrace: {ex.StackTrace} | Desscription: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        public ResponseData MakeTransaction(long coinsQuantity, string description)
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
                if (!response.IsSuccesful)
                {
                    _logger.LogError($"ErrorDescription: {response.ErrorResponse.ErrorDescription} |ErrorMessage: {response.ErrorResponse.ErrorMessage}");
                    response.IsSuccesful = false;
                    if (response.ErrorResponse.ErrorDescription == Error.INVALID_TOKEN) //dispose invalid refresh token
                        _session.RefreshToken = "";
                }

                _logger.LogInformation(response.ToJSONString());
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error MakeTransaction: {ex.Message} | Stacktrace: {ex.StackTrace} | Desscription: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        public string GetBuyQuartersUrl()
        {
            try
            {
                _logger.LogInformation("Buy Quarters");

                string redirectSafeUrl = HttpUtility.UrlEncode(_app.SCHEMA_URL);
                return $"{_api.BuyURL}?redirect={redirectSafeUrl}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error GetBuyQuartersUrl: {ex.Message} | Stacktrace: {ex.StackTrace} | Desscription: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        public void SignOut()
        {
            try
            {
                _session.Invalidate();
                _logger.LogInformation("Quarters user signed out");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error SignOut: {ex.Message} | Stacktrace: {ex.StackTrace} | Desscription: {ex.InnerException}");
                throw new Error(ex.Message, ex.InnerException.ToString());
            }
        }

        #endregion UNITY Methods
    }
}