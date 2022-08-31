using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using QuartersSDK.Services;
using QuartersSDK.Data;
using Microsoft.Extensions.Logging;
using Packages.gg.poq.unity.sdk.Runtime.QuartersSDK.Scripts.FileLogger;
using System.IO;
using Packages.gg.poq.unity.sdk.Runtime.QuartersSDK.Scripts.ConfigurationSettings;
using QuartersSDK.UI;

namespace QuartersSDK {

    public class QuartersController : MonoBehaviour {
        private Quarters _quarters;
        private User _currentUser;
        public Session _session;

        public static QuartersController Instance;
        public static Action<User> OnUserLoaded;
        public static Action<long> OnBalanceUpdated;
        public static Action OnSignOut;
        public static string URL_SCHEME = string.Empty;

        public CurrencyConfig CurrencyConfig => QuartersInit.Instance.CurrencyConfig;
        public List<Scope> DefaultScope = new List<Scope> {
            Scope.identity,
            Scope.email,
            Scope.transactions,
            Scope.events,
            Scope.wallet
        };

        [HideInInspector] 
        public QuartersWebView QuartersWebView;

        /// <summary>
        /// Get or set the current user
        /// </summary>
        public User CurrentUser {
            get => _currentUser;
            set {
                _currentUser = value;
                if (value != null) OnUserLoaded?.Invoke(_currentUser);
            }
        }

        /// <summary>
        /// Returns true in case the session is valid
        /// </summary>
        public bool IsAuthorized() {  
            return _session != null && _session.IsAuthorized;
        }

        /// <summary>
        /// Initialization of Quarters user session and instance
        /// </summary>
        public void Init() 
        {
            try
            {
                Instance = this;
                _session = new Session();

                var loggerFactory = (ILoggerFactory)new LoggerFactory();
#if UNITY_EDITOR
                loggerFactory.AddFile(Path.Combine(Directory.GetCurrentDirectory(), "logs"));
#endif
                var logger = loggerFactory.CreateLogger<Quarters>();

                APIClient apiClient = new APIClient(logger);
                var apiParamsSettings = new ApiParamsSettings();
                var appParamsSettings = new AppParamsSettings(QuartersInit.Instance.APP_ID, 
                                                            QuartersInit.Instance.APP_KEY, 
                                                            QuartersInit.Instance.APP_UNIQUE_IDENTIFIER, 
                                                            "Production");
                _quarters = new Quarters(apiClient, logger, _session.RefreshToken, appParamsSettings.Settings, apiParamsSettings.Settings);
                URL_SCHEME = appParamsSettings.Settings["REDIRECT_URL"];
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.InnerException}");
                ModalView.instance.ShowAlert("QuartersController|Init|Error", $"{ex.Message} \n {ex.StackTrace} ", new string[] { "OK" }, null);
            }

        }

        #region high level calls

        /// <summary>
        /// This method authorizes the user and signing then 
        /// </summary>
        /// <param name="OnComplete">What happens when signing in is successful</param>
        /// <param name="OnError">Checks to see if there was an error in signing in</param>
        public void SignInWithQuarters(Action OnComplete, Action<string> OnError) 
        {
            try
            {
                _session = new Session();
                _session.Scopes = DefaultScope;
                Instance.Authorize(_session.Scopes, delegate
                {
                    OnComplete?.Invoke();
                    VspAttribution.VspAttribution.SendAttributionEvent("SignInWithQuarters", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                }, OnError);
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.InnerException}");
                OnError?.Invoke(ex.Message + ex.InnerException + ex.StackTrace);
            }
        }

        private void Authorize(List<Scope> scopes, Action OnSuccess, Action<string> OnError) 
        {
            try
            {
                _session = new Session();

                if (IsAuthorized())
                {
                    GetAccessToken(delegate { OnSuccess?.Invoke(); }, delegate (string error) { OnError?.Invoke(error); });
                    return;
                }

                string scopeString = "";
                foreach (Scope scope in scopes)
                {
                    scopeString += scope.ToString();
                    if (scopes.IndexOf(scope) != scopes.Count - 1) scopeString += " ";
                }

                string url = _quarters.GetAuthorizeUrl();
                Log(url);

                //web view authentication
                LinkType linkType = Application.platform == RuntimePlatform.WindowsEditor
                    ? LinkType.EditorExternal
                    : LinkType.External;

                QuartersWebView.OpenURL(url, linkType);
                QuartersWebView.OnDeepLink = delegate (QuartersLink link)
                {
                    if (link.QueryString.ContainsKey("code"))
                    {
                        string code = link.QueryString["code"];
                        GetRefreshToken(code, OnSuccess, OnError);
                        QuartersWebView.OnDeepLink = null;
                        VspAttribution.VspAttribution.SendAttributionEvent("QuartersAuthorize", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                    }
                    else if (link.QueryString.ContainsKey("error"))
                    {
                        OnError?.Invoke(link.QueryString["error"]);
                        QuartersWebView.OnDeepLink = null;
                    }
                };
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.InnerException}");
                OnError?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Deauthorization and signing out
        /// </summary>
        public void Deauthorize() 
        {
            try
            {
                _quarters.SignOut();
                _session.SignOut();
                _session = null;

                Log("Quarters user signed out");
                OnSignOut?.Invoke();

                VspAttribution.VspAttribution.SendAttributionEvent("QuartersDeauthorize", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.InnerException}");
                throw ex;
            }
        }

        /// <summary>
        /// Gets the user's account details
        /// </summary>
        /// <param name="OnSuccessDelegate">Operation was successful and details have been retreived</param>
        /// <param name="OnFailedDelegate">Operation failed and details have not been retreived</param>
        public void GetUserDetails(Action<User> OnSuccessDelegate, Action<string> OnFailedDelegate) 
        {
            try
            {
                StartCoroutine(GetUserDetailsCall(OnSuccessDelegate, OnFailedDelegate));
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.InnerException}");
                OnFailedDelegate?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Gets the user's account balance (The amount of quarters they have)
        /// </summary>
        /// <param name="OnSuccess">Operation was successful and the account balance information has been retreived</param>
        /// <param name="OnError">Operation failed and the account balance information has not been retreived</param>
        public void GetAccountBalanceCall(Action<long> OnSuccess, Action<string> OnError) 
        {
            try
            {
                StartCoroutine(GetAccountBalance(OnSuccess, OnError));
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.InnerException}");
                OnError?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Handles quarters transactions. 
        /// After calling this method the user will receive the amount of @coinsQuantity and the reason of transaction will be @description
        /// </summary>
        /// <param name="coinsQuantity">How many quarters are involved in the transaction</param>
        /// <param name="description">Description of what the transaction is</param>
        /// <param name="OnSuccess">The transaction was successful</param>
        /// <param name="OnError">There was an error in the transaction</param>
        public IEnumerator Transaction(long coinsQuantity, string description, Action OnSuccess, Action<string> OnError) 
        {
            yield return StartCoroutine(MakeTransaction(coinsQuantity, description, OnSuccess, OnError));
        }

        #endregion

        #region api calls

        private void GetRefreshToken(string code, Action OnComplete, Action<string> OnError) 
        {
            Log($"Get refresh token with code: {code}");
            try
            {
                var response = _quarters.GetRefreshToken(code);
                if (response.IsSuccesful)
                {
                    Log($"GetRefreshToken result: {response.ToJSONString()}");

                    _session.RefreshToken = response.RefreshToken;
                    _session.AccessToken = response.AccessToken;
                    _session.SetScope(response.Scope);
                    OnComplete?.Invoke();
                }
                else
                {
                    LogError($"Message: {response.ErrorResponse.ErrorMessage} | Description: {response.ErrorResponse.ErrorDescription}");
                    OnError?.Invoke(response.ErrorResponse.ErrorDescription);
                }
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.InnerException}");
                OnError?.Invoke(ex.Message);
            }
        }

        private void GetAccessToken(Action OnSuccess, Action<string> OnFailed) 
        {
            Log("Get Access token");
            try
            {
                if (!_session.DoesHaveRefreshToken)
                {
                    LogError("Missing refresh token");
                    OnFailed("Missing refresh token");
                }
                
                _quarters._session.RefreshToken = _session.RefreshToken;
                ResponseData response = _quarters.GetAccessToken();
                if (response.IsSuccesful)
                {
                    Log($"GetAccessToken result: {response.ToJSONString()}");

                    _session.RefreshToken = response.RefreshToken;
                    _session.AccessToken = response.AccessToken;
                    _session.SetScope(response.Scope);
                    OnSuccess?.Invoke();
                }
                else
                {
                    LogError($"Message: {response.ErrorResponse.Message} | Description: {response.ErrorResponse.ErrorDescription}");

                    if (response.ErrorResponse.ErrorDescription == Error.INVALID_TOKEN) //dispose invalid refresh token
                        _session.RefreshToken = "";

                    OnFailed?.Invoke(response.ErrorResponse.ErrorDescription);
                }
            }
            catch(Exception ex)
            {
                LogError(ex.Message);
                OnFailed?.Invoke(ex.Message);
            }
        }

        private IEnumerator GetUserDetailsCall(Action<User> OnSuccess, Action<string> OnFailed, bool isRetry = false) 
        {
            Log("GetUserDetailsCall");
            try
            {
                if (!_session.DoesHaveAccessToken)
                {
                    GetAccessToken(OnSuccess: delegate { StartCoroutine(GetUserDetailsCall(OnSuccess, OnFailed, true)); },
                                    OnFailed: delegate (string error) { OnFailed(error); });
                    yield break;
                }

                var userStr = _quarters.GetUserDetailsCallStr();
                if (string.IsNullOrEmpty(userStr))
                {
                    LogError($"Error GetUserDetailsCall: {userStr} ");

                    //refresh access code and retry this call in case access code expired
                    if (!isRetry)
                        GetAccessToken(delegate { StartCoroutine(GetUserDetailsCall(OnSuccess, OnFailed, true)); }, delegate { OnFailed(userStr); });
                    else
                        OnFailed($"Error GetUserDetailsCall: {userStr}");
                }
                else
                {
                    Log("GetUserDetailsCall result " + userStr);

                    CurrentUser = JsonConvert.DeserializeObject<User>(userStr);
                    OnSuccess(CurrentUser);
                }
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.StackTrace}");
                OnFailed?.Invoke(ex.Message);
            }
        }


        private IEnumerator GetAccountBalance(Action<long> OnSuccess, Action<string> OnFailed, bool isRetry = false) 
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.GamerTag)) 
                    yield return GetUserDetailsCall(delegate { }, OnFailed);
            try
            {
                long balance = _quarters.GetAccountBalanceCall();

                Log($"GetAccountBalance:{balance}");
                CurrentUser.Balance = balance;
                OnBalanceUpdated?.Invoke(CurrentUser.Balance);
                OnSuccess(CurrentUser.Balance);
            }
            catch (Exception ex)
            {
                LogError($"QuartersController| GetAccountBalance |Message: {ex.Message} | Description: {ex.StackTrace}");
                OnFailed(ex.Message);
            }

        }


        /// <summary>
        /// After calling this method the user will receive the amount of @coinsQuantity and the reason of transaction will be @description
        /// </summary>
        /// <param name="coinsQuantity">How many quarters are involved in the transaction</param>
        /// <param name="description">Description of what the transaction is</param>
        /// <param name="OnSuccess">The transaction was successful</param>
        /// <param name="OnError">There was an error in the transaction</param>
        public IEnumerator MakeTransaction(long coinsQuantity, string description, Action OnSuccess, Action<string> OnFailed) 
        {
            Log($"MakeTransaction with quantity: {coinsQuantity}");
            try
            {
                if (!_session.DoesHaveRefreshToken)
                {
                    LogError("Missing refresh token");
                    OnFailed("Missing refresh token");
                    yield break;
                }

                var response = _quarters.MakeTransaction(coinsQuantity, description);
                if (response.IsSuccesful)
                {
                    Log(response.ToJSONString());
                    VspAttribution.VspAttribution.SendAttributionEvent("TransactionSuccess", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                    GetAccountBalanceCall(delegate { OnSuccess?.Invoke(); }, OnFailed);
                }
                else
                {
                    LogError($"Message: {response.ErrorResponse.Message} | Description: {response.ErrorResponse.ErrorDescription}");

                    if (response.ErrorResponse.ErrorDescription == Error.INVALID_TOKEN) //dispose invalid refresh token
                        _session.RefreshToken = "";
                    OnFailed?.Invoke(response.ErrorResponse.ErrorDescription);
                }
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.InnerException}");
                OnFailed?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// When you call this method, the user will be sent to a website where they can exchange money for quarters
        /// </summary>
        public void BuyQuarters() 
        {
            Log("Buy Quarters");
            try {
                VspAttribution.VspAttribution.SendAttributionEvent("BuyQuarters", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                QuartersWebView.OpenURL(_quarters.GetBuyQuartersUrl(), LinkType.External);
            }
            catch (Exception ex)
            {
                LogError($"Message: {ex.Message} | Description: {ex.InnerException}");
                throw ex;
            }
        }

        #endregion

        private void Log(string message) 
        {
            if (QuartersInit.Instance.ConsoleLogging == QuartersInit.LoggingType.Verbose) {
                Debug.Log(message);
            }
        }

        private void LogError(string message) 
        {
            if (QuartersInit.Instance.ConsoleLogging == QuartersInit.LoggingType.Verbose) {
                Debug.LogError(message);
            }
        }
    }
}


