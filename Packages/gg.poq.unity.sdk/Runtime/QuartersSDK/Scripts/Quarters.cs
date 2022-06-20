using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QuartersSDK {
    public class Quarters : MonoBehaviour {
        public static Action<User> OnUserLoaded;
        public static Action<long> OnBalanceUpdated;
        public static Action OnSignOut;

        public static Quarters Instance;

        public static string URL_SCHEME = string.Empty;

        private User currentUser;

        public List<Scope> DefaultScope = new List<Scope> {
            Scope.identity,
            Scope.email,
            Scope.transactions,
            Scope.events,
            Scope.wallet
        };

        private PCKE pcke;
        [HideInInspector] public QuartersWebView QuartersWebView;

        public Session session;

        public CurrencyConfig CurrencyConfig => QuartersInit.Instance.CurrencyConfig;
        

        /// <summary>
        /// Returns the URL of the environment we set
        /// </summary>
        public string BASE_URL {
            get {
                Environment environment = QuartersInit.Instance.Environment;
                if (environment == Environment.production) return "https://www.poq.gg";
                if (environment == Environment.sandbox) return "https://s2w-dev-firebase.herokuapp.com";
                return null;
            }
        }

        /// <summary>
        /// Gets the base API endpoint
        /// </summary>
        public string API_URL => $"{BASE_URL}/api/v1";

        /// <summary>
        /// Gets the buy endpoint
        /// </summary>
        public string BUY_URL => $"{BASE_URL}/buy";

        /// <summary>
        /// Get or set the current user
        /// </summary>
        public User CurrentUser {
            get => currentUser;
            set {
                currentUser = value;
                if (value != null) OnUserLoaded?.Invoke(currentUser);
            }
        }

        /// <summary>
        /// Returns true in case the session is valid
        /// </summary>
        public bool IsAuthorized {
            get {
                if (session != null)
                    return session.IsAuthorized;
                return false;
            }
        }

        /// <summary>
        /// Initialization of Quarters user session and instance
        /// </summary>
        public void Init() {
            Instance = this;
            session = new Session();

            pcke = new PCKE();
            URL_SCHEME = $"https://{QuartersInit.Instance.APP_UNIQUE_IDENTIFIER}.games.poq.gg";
        }

        #region high level calls

        /// <summary>
        /// This method authorizes the user and signing then 
        /// </summary>
        /// <param name="OnComplete">What happens when signing in is successful</param>
        /// <param name="OnError">Checks to see if there was an error in signing in</param>
        public void SignInWithQuarters(Action OnComplete, Action<string> OnError) {
            Session session = new Session();
            session.Scopes = DefaultScope;
            Instance.Authorize(session.Scopes, delegate {
                OnComplete?.Invoke();
                try {
                    VspAttribution.VspAttribution.SendAttributionEvent("SignInWithQuarters", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                }
                catch (Exception e) {
   
                }
            }, OnError);
        }


        private void Authorize(List<Scope> scopes, Action OnSuccess, Action<string> OnError) {
            session = new Session();


            if (IsAuthorized) {
                StartCoroutine(GetAccessToken(delegate { OnSuccess?.Invoke(); }, delegate(string error) { OnError?.Invoke(error); }));

                return;
            }


            string redirectSafeUrl = UnityWebRequest.EscapeURL(URL_SCHEME);

            string scopeString = "";
            foreach (Scope scope in scopes) {
                scopeString += scope.ToString();
                if (scopes.IndexOf(scope) != scopes.Count - 1) scopeString += " ";
            }

            string url = BASE_URL + "/oauth2/authorize?response_type=code&client_id="
                                  + QuartersInit.Instance.APP_ID + "&redirect_uri="
                                  + redirectSafeUrl
                                  + $"&scope={UnityWebRequest.EscapeURL(scopeString)}"
                                  + "&code_challenge_method=S256"
                                  + $"&code_challenge={pcke.CodeChallenge()}";

            Log(url);

            //web view authentication
            LinkType linkType = Application.platform == RuntimePlatform.WindowsEditor
                ? LinkType.EditorExternal
                : LinkType.External;


            QuartersWebView.OpenURL(url, linkType);
            QuartersWebView.OnDeepLink = delegate(QuartersLink link) {
                if (link.QueryString.ContainsKey("code")) {
                    string code = link.QueryString["code"];
                    StartCoroutine(GetRefreshToken(code, OnSuccess, OnError));
                    QuartersWebView.OnDeepLink = null;
                    
                    try {
                        VspAttribution.VspAttribution.SendAttributionEvent("QuartersAuthorize", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                    }
                    catch (Exception e) {
   
                    }
                }
                else if (link.QueryString.ContainsKey("error")) {
                    OnError?.Invoke(link.QueryString["error"]);
                    QuartersWebView.OnDeepLink = null;
                }
            };
        }

        /// <summary>
        /// Deauthorization and signing out
        /// </summary>
        public void Deauthorize() {
            Session.Invalidate();
            session = null;
            CurrentUser = null;

            Log("Quarters user signed out");
            OnSignOut?.Invoke();
            
            try {
                VspAttribution.VspAttribution.SendAttributionEvent("QuartersDeauthorize", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
            }
            catch (Exception e) {
   
            }
        }

        /// <summary>
        /// Gets the user's account details
        /// </summary>
        /// <param name="OnSuccessDelegate">Operation was successful and details have been retreived</param>
        /// <param name="OnFailedDelegate">Operation failed and details have not been retreived</param>
        public void GetUserDetails(Action<User> OnSuccessDelegate, Action<string> OnFailedDelegate) {
            StartCoroutine(GetUserDetailsCall(OnSuccessDelegate, OnFailedDelegate));
        }

        /// <summary>
        /// Gets the user's account balance (The amount of quarters they have)
        /// </summary>
        /// <param name="OnSuccess">Operation was successful and the account balance information has been retreived</param>
        /// <param name="OnError">Operation failed and the account balance information has not been retreived</param>
        public void GetAccountBalanceCall(Action<long> OnSuccess, Action<string> OnError) {
            StartCoroutine(GetAccountBalance(OnSuccess, OnError));
        }

        /// <summary>
        /// Handles quarters transactions. 
        /// After calling this method the user will receive the amount of @coinsQuantity and the reason of transaction will be @description
        /// </summary>
        /// <param name="coinsQuantity">How many quarters are involved in the transaction</param>
        /// <param name="description">Description of what the transaction is</param>
        /// <param name="OnSuccess">The transaction was successful</param>
        /// <param name="OnError">There was an error in the transaction</param>
        public void Transaction(long coinsQuantity, string description, Action OnSuccess, Action<string> OnError) {
            StartCoroutine(MakeTransaction(coinsQuantity, description, OnSuccess, OnError));
        }

        #endregion

        #region api calls

        private IEnumerator GetRefreshToken(string code, Action OnComplete, Action<string> OnError) {
            Log($"Get refresh token with code: {code}");

            WWWForm data = new WWWForm();
            data.AddField("code_verifier", pcke.CodeVerifier);
            data.AddField("client_id", QuartersInit.Instance.APP_ID);
            data.AddField("grant_type", "authorization_code");
            data.AddField("code", code);
            data.AddField("redirect_uri", URL_SCHEME);


            string url = BASE_URL + "/api/oauth2/token";
            Log("GetRefreshToken url: " + url);

            using (UnityWebRequest request = UnityWebRequest.Post(url, data)) {
                yield return request.SendWebRequest();


                if (request.isNetworkError || request.isHttpError) {
                    LogError(request.error);
                    LogError(request.downloadHandler.text);

                    OnError(request.error);
                }
                else {
                    Log(request.downloadHandler.text);

                    Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
                    session.RefreshToken = responseData["refresh_token"];
                    session.AccessToken = responseData["access_token"];
                    session.SetScope(responseData["scope"]);

                    OnComplete();
                }
            }
        }


        private IEnumerator GetAccessToken(Action OnSuccess, Action<string> OnFailed) {
            Log("Get Access token");

            if (!session.DoesHaveRefreshToken) {
                LogError("Missing refresh token");
                OnFailed("Missing refresh token");
                yield break;
            }

            WWWForm data = new WWWForm();
            data.AddField("grant_type", "refresh_token");
            data.AddField("client_id", QuartersInit.Instance.APP_ID);
            data.AddField("client_secret", QuartersInit.Instance.APP_KEY);
            data.AddField("refresh_token", session.RefreshToken);
            data.AddField("code_verifier", pcke.CodeVerifier);

            string url = BASE_URL + "/api/oauth2/token";
            Log("GetAccessToken url: " + url);

            using (UnityWebRequest request = UnityWebRequest.Post(url, data)) {
                yield return request.SendWebRequest();


                if (request.isNetworkError || request.isHttpError) {
                    LogError(request.error);
                    LogError(request.downloadHandler.text);

                    Error error = new Error(request.downloadHandler.text);

                    if (error.ErrorDescription == Error.INVALID_TOKEN) //dispose invalid refresh token
                        session.RefreshToken = "";

                    OnFailed?.Invoke(error.ErrorDescription);
                }
                else {
                    Log("GetAccessToken result " + request.downloadHandler.text);

                    Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
                    session.RefreshToken = responseData["refresh_token"];
                    session.AccessToken = responseData["access_token"];
                    session.SetScope(responseData["scope"]);
                    OnSuccess?.Invoke();
                }
            }
        }

        private IEnumerator GetUserDetailsCall(Action<User> OnSuccess, Action<string> OnFailed, bool isRetry = false) {
            Log("GetUserDetailsCall");

            if (!session.DoesHaveAccessToken) {
                StartCoroutine(GetAccessToken(delegate { StartCoroutine(GetUserDetailsCall(OnSuccess, OnFailed, true)); }, delegate(string error) { OnFailed(error); }));
                yield break;
            }

            string url = API_URL + "/users/me";
            Log(url);

            using (UnityWebRequest request = UnityWebRequest.Get(url)) {
                request.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");
                request.SetRequestHeader("Authorization", "Bearer " + session.AccessToken);
                // Request and wait for the desired page.
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError) {
                    LogError(request.error);
                    LogError(request.downloadHandler.text);

                    if (!isRetry) {
                        //refresh access code and retry this call in case access code expired
                        StartCoroutine(GetAccessToken(delegate { StartCoroutine(GetUserDetailsCall(OnSuccess, OnFailed, true)); }, delegate { OnFailed(request.error); }));
                    }
                    else {
                        LogError(request.error);
                        OnFailed(request.error);
                    }
                }
                else {
                    Log("GetUserDetailsCall result " + request.downloadHandler.text);

                    CurrentUser = JsonConvert.DeserializeObject<User>(request.downloadHandler.text);
                    OnSuccess(CurrentUser);
                }
            }
        }


        private IEnumerator GetAccountBalance(Action<long> OnSuccess, Action<string> OnFailed, bool isRetry = false) {
            if (CurrentUser == null) yield return GetUserDetailsCall(delegate { }, OnFailed);

            string url = API_URL + "/wallets/@me";

            using (UnityWebRequest request = UnityWebRequest.Get(url)) {
                request.SetRequestHeader("Authorization", "Bearer " + session.AccessToken);
                // Request and wait for the desired page.
                yield return request.SendWebRequest();


                if (request.isNetworkError || request.isHttpError) {
                    LogError(request.error);
                    LogError(request.downloadHandler.text);

                    if (!isRetry) {
                        //refresh access code and retry this call in case access code expired
                        StartCoroutine(GetAccessToken(delegate { StartCoroutine(GetAccountBalance(OnSuccess, OnFailed, true)); }, delegate { OnFailed(request.error); }));
                    }
                    else {
                        LogError(request.error);
                        OnFailed(request.error);
                    }
                }
                else {
                    Log(request.downloadHandler.text);
                    JObject responseData = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
                    CurrentUser.Balance = responseData["balance"].ToObject<long>();
                    OnBalanceUpdated?.Invoke(CurrentUser.Balance);
                    OnSuccess(CurrentUser.Balance);
                }
            }
        }


        /// <summary>
        /// After calling this method the user will receive the amount of @coinsQuantity and the reason of transaction will be @description
        /// </summary>
        /// <param name="coinsQuantity">How many quarters are involved in the transaction</param>
        /// <param name="description">Description of what the transaction is</param>
        /// <param name="OnSuccess">The transaction was successful</param>
        /// <param name="OnError">There was an error in the transaction</param>
        public IEnumerator MakeTransaction(long coinsQuantity, string description, Action OnSuccess, Action<string> OnFailed) {
            Log($"MakeTransaction with quantity: {coinsQuantity}");

            if (!session.DoesHaveRefreshToken) {
                LogError("Missing refresh token");
                OnFailed("Missing refresh token");
                yield break;
            }

            string url = API_URL + "/transactions";
            Log("Transaction url: " + url);

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("creditUser", coinsQuantity);
            postData.Add("description", description);

            string json = JsonConvert.SerializeObject(postData);


            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] jsonToSend = new UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + session.AccessToken);
            request.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError) {
                LogError(request.error);
                LogError(request.downloadHandler.text);

                Error error = new Error(request.downloadHandler.text);

                if (error.ErrorDescription == Error.INVALID_TOKEN) //dispose invalid refresh token
                    session.RefreshToken = "";

                VspAttribution.VspAttribution.SendAttributionEvent("TransactionFailed", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                OnFailed?.Invoke(error.ErrorDescription);
            }
            else if (request.isDone)
            {
                Log(request.downloadHandler.text);
                VspAttribution.VspAttribution.SendAttributionEvent("TransactionSuccess", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
                GetAccountBalanceCall(delegate { OnSuccess?.Invoke(); }, OnFailed);
            }
        }

        /// <summary>
        /// When you call this method, the user will be sent to a website where they can exchange money for quarters
        /// </summary>
        public void BuyQuarters() {
            Log("Buy Quarters");

            string redirectSafeUrl = UnityWebRequest.EscapeURL(URL_SCHEME);

            string url = $"{BUY_URL}?redirect={redirectSafeUrl}";

            try {
                VspAttribution.VspAttribution.SendAttributionEvent("BuyQuarters", Constants.VSP_POQ_COMPANY_NAME, QuartersInit.Instance.APP_ID);
            }
            catch (Exception e) {
   
            }
            
            QuartersWebView.OpenURL(url, LinkType.External);
        }

        #endregion
        
        private void Log(string message) {
            if (QuartersInit.Instance.ConsoleLogging == QuartersInit.LoggingType.Verbose) {
                Debug.Log(message);
            }
        }
        
        private void LogError(string message) {
            if (QuartersInit.Instance.ConsoleLogging == QuartersInit.LoggingType.Verbose) {
                Debug.LogError(message);
            }
        }
    }
}
