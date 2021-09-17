using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace QuartersSDK {
	public partial class Quarters : MonoBehaviour {
        
        public static Action<User> OnUserLoaded;

		public static Quarters Instance;
        private Session session;
        private PCKE PCKE;

        [HideInInspector] public UniWebView WebView;
        
        
        private CurrencyConfig currencyConfig;
        public CurrencyConfig CurrencyConfig {
            get {
                return QuartersInit.Instance.CurrencyConfig;
            }
        }

        public delegate void OnAuthorizationStartDelegate();
		public static event OnAuthorizationStartDelegate OnAuthorizationStart;

		public delegate void OnAuthorizationSuccessDelegate();
		public OnAuthorizationSuccessDelegate OnAuthorizationSuccess;

		public delegate void OnAuthorizationFailedDelegate(string error);
		public OnAuthorizationFailedDelegate OnAuthorizationFailed;

		public delegate void OnUserDetailsSucessDelegate(User user);
		public delegate void OnUserDetailsFailedDelegate(string error);
        

        //Transfer
        public delegate void OnTransferSuccessDelegate(string transactionHash);
        public delegate void OnTransferFailedDelegate(string error);

        private Rect WebViewFrame {
            get {
                float factor = Application.isEditor ? 0.3f : 1f;
                return new Rect(0, 0, (float) Screen.width * factor, (float) Screen.height * factor);
            }
        }


        public List<TransferAPIRequest> currentTransferAPIRequests = new List<TransferAPIRequest>();

		public string BASE_URL {
			get {
                Environment environment = QuartersInit.Instance.environment;
                if (environment == Environment.production) return $"https://{CurrencyConfig.APIBaseUrl}";
                else if (environment == Environment.development) return $"https://dev.{CurrencyConfig.APIBaseUrl}";
                else if (environment == Environment.sandbox) return $"https://sandbox.{CurrencyConfig.APIBaseUrl}";
                return null;
            }
		}

		public string API_URL {
			get {
               
                return $"{BASE_URL}/v1";
			}
		}


        public static string URL_SCHEME  {
            get {
                return "https://quarters-sandbox.s3.us-east-2.amazonaws.com/";
   
            }
        }


		private User currentUser = null;
		public User CurrentUser {
			get {
				return currentUser;
			}
			set {
				currentUser = value;
                if (value != null) {
                    OnUserLoaded?.Invoke(currentUser);
                }
            }
		}


	

        public bool IsAuthorized {
            get {
                if (session != null) {
                    return session.IsAuthorized;
                } else return false;
            }
        }


		

		public void Init() {
			Instance = this;
            PCKE = new PCKE();
        }



        #region high level calls


        public void Authorize(List<Scope> scopes, OnAuthorizationSuccessDelegate OnSuccessDelegate, OnAuthorizationFailedDelegate OnFailedDelegate) {
            
            session = new Session();

			this.OnAuthorizationSuccess = OnSuccessDelegate;
			this.OnAuthorizationFailed = OnFailedDelegate;

            if (IsAuthorized) {

                StartCoroutine(GetAccessToken(delegate {
                    OnSuccessDelegate?.Invoke();
                }, delegate(string error) {
                    OnFailedDelegate?.Invoke(error);
                }));
                
                return;
            }

			if (OnAuthorizationStart != null) OnAuthorizationStart();


            Debug.Log("OAuth authorization");
            
            // string redirectSafeUrl = UnityWebRequest.EscapeURL(URL_SCHEME);

            string scopeString = "";
            foreach (Scope scope in scopes) {
                scopeString += scope.ToString();
                if (scopes.IndexOf(scope) != scopes.Count - 1) {
                    scopeString += " ";
                }
            }

            string url = BASE_URL + "/oauth2/authorize?response_type=code&client_id="
                                  + QuartersInit.Instance.APP_ID + "&redirect_uri="
                                  + $"&scope={UnityWebRequest.EscapeURL(scopeString)}"
                                  + $"&code_challenge_method=S256"
                                  + $"&code_challenge={PCKE.CodeChallenge()}";
                                
            Debug.Log(url);
            

            //webview authentication
            WebView = this.gameObject.AddComponent<UniWebView>();
      
            WebView.Frame = WebViewFrame;
            WebView.Load(url);
            WebView.Show(false, UniWebViewTransitionEdge.Bottom, 0.33f);
            
            WebView.OnPageStarted += delegate(UniWebView view, string pageUrl) {
                
                Debug.Log(pageUrl);
                
                UniWebViewMessage message = new UniWebViewMessage(pageUrl);

                if (message.Args.ContainsKey("code")) {
                    AuthorizationCodeReceived(message.Args["code"]);
                    WebView.Hide();
                }
                else if (message.Args.ContainsKey("error")) {
                    ModalView.instance.ShowAlert("Authorisation error", message.Args["error_description"], new []{"OK"}, null);
                    WebView.Hide();
                }
            }; 
            
   
        }
        

    
        

        public void Deauthorize() {
            Session.Invalidate();
            this.session = null;
            CurrentUser = null;

            //clean up delegates
            OnAuthorizationStart = null;
            OnAuthorizationSuccess = null;
            OnAuthorizationFailed = null;
            currentTransferAPIRequests = new List<TransferAPIRequest>();
            
        }




		public void GetUserDetails(OnUserDetailsSucessDelegate OnSuccessDelegate, OnUserDetailsFailedDelegate OnFailedDelegate) {
            StartCoroutine(GetUserDetailsCall(OnSuccessDelegate, OnFailedDelegate));
		}

        public void GetAccountBalanceCall(Action<long> OnSuccess, Action<string> OnError) {
            StartCoroutine(GetAccountBalance(OnSuccess, OnError));
        }

        
        public void MakeTransactionCall(long coinsQuantity, string description, Action OnSuccess, Action<string> OnError) {
            StartCoroutine(MakeTransaction(coinsQuantity, description, OnSuccess, OnError));
        }


        

        #endregion

    
		public void AuthorizationCodeReceived(string code) {

			Debug.Log("Quarters: Authorization code: " + code);
            
			StartCoroutine(GetRefreshToken(code));
        }
        
   


        #region api calls

        

		public IEnumerator GetRefreshToken(string code) {

            Debug.Log($"Get refresh token with code: {code}");

			WWWForm data = new WWWForm();
            data.AddField("code_verifier", PCKE.CodeVerifier);
            data.AddField("client_id", QuartersInit.Instance.APP_ID);
            data.AddField("grant_type", "authorization_code");
			data.AddField("code", code);
            // data.AddField("redirect_uri", URL_SCHEME);
           
            
            string url = BASE_URL + "/oauth2/token";
            Debug.Log("GetRefreshToken url: " + url);

            using (UnityWebRequest request = UnityWebRequest.Post(url, data)) {
                yield return request.SendWebRequest();


                if (request.isNetworkError || request.isHttpError) {
                    Debug.LogError(request.error);
                    Debug.LogError(request.downloadHandler.text);
                    
                    OnAuthorizationFailed(request.error);
                }
                else {
                    Debug.Log(request.downloadHandler.text);
                    
                    Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
                    session.RefreshToken = responseData["refresh_token"];
                    session.AccessToken = responseData["access_token"];
                    session.SetScope(responseData["scope"]);

                    OnAuthorizationSuccess();
                }
            }
        }
        
        


        public IEnumerator GetAccessToken(Action OnSuccess, Action<string> OnFailed) {
            
            Debug.Log("Get Access token");
            
            if (!session.DoesHaveRefreshToken) {
                Debug.LogError("Missing refresh token");
                OnFailed("Missing refresh token");
                yield break;
            }
            
            
            WWWForm data = new WWWForm();
            data.AddField("grant_type", "refresh_token");
            data.AddField("client_id", QuartersInit.Instance.APP_ID);
            data.AddField("client_secret", QuartersInit.Instance.APP_KEY);
            data.AddField("refresh_token", session.RefreshToken);
            data.AddField("code_verifier", PCKE.CodeVerifier);
            
            string url = BASE_URL + "/oauth2/token";
            Debug.Log("GetAccessToken url: " + url);

            using (UnityWebRequest request = UnityWebRequest.Post(url, data)) {
                yield return request.SendWebRequest();


                if (request.isNetworkError || request.isHttpError) {
                    Debug.LogError(request.error);
                    Debug.LogError(request.downloadHandler.text);
                    
                    Error error = new Error(request.downloadHandler.text);

                    if (error.ErrorDescription == Error.INVALID_TOKEN) {
                        //dispose invalid refresh token
                        session.RefreshToken = "";
                    }
                    
                    OnFailed?.Invoke(error.ErrorDescription);
                }
                else {
                    Debug.Log("GetAccessToken result " + request.downloadHandler.text);
                    
                    Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
                    session.RefreshToken = responseData["refresh_token"];
                    session.AccessToken = responseData["access_token"];
                    session.SetScope(responseData["scope"]);
                    OnSuccess?.Invoke();
                }
            }
        }

        
        
        public IEnumerator GetAvatar(Action<Texture> OnSuccess, Action<Error> OnError) {

            string url = $"https://www.poq.gg/images/{CurrentUser.Id}/{CurrentUser.AvatarUrl}";
            Debug.Log($"Pull avatar: {url}");
            
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError) {
                Debug.LogError(www.error);
                Debug.LogError(www.downloadHandler.text);
                
                Error error = new Error(www.downloadHandler.text);
                
                OnError?.Invoke(error);
            }
            else {
                Texture avatarTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                OnSuccess?.Invoke(avatarTexture);
            }
        }


        private IEnumerator GetUserDetailsCall(OnUserDetailsSucessDelegate OnSuccess, OnUserDetailsFailedDelegate OnFailed, bool isRetry = false) {

            Debug.Log("GetUserDetailsCall");

            if (!session.DoesHaveAccessToken) {
                StartCoroutine(GetAccessToken(delegate {
                    StartCoroutine(GetUserDetailsCall(OnSuccess, OnFailed, true));
                }, delegate(string error) {
                    OnFailed(error);
                }));
                yield break;
            }

        
            string url = API_URL + "/users/me";
            Debug.Log(url);

            using (UnityWebRequest request = UnityWebRequest.Get(url)) {
                request.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");
                request.SetRequestHeader("Authorization", "Bearer " + session.AccessToken);
                // Request and wait for the desired page.
                yield return request.SendWebRequest();


                if (request.isNetworkError || request.isHttpError) {
                    Debug.LogError(request.error);
                    Debug.LogError(request.downloadHandler.text);

                    if (!isRetry) {
                        //refresh access code and retry this call in case access code expired
                        StartCoroutine(GetAccessToken(delegate {
                            StartCoroutine(GetUserDetailsCall(OnSuccess, OnFailed, true));
                        }, delegate(string error) { OnFailed(request.error); }));
                    }
                    else {
                        Debug.LogError(request.error);
                        OnFailed(request.error);
                    }
                }
                else {
                    Debug.Log("GetUserDetailsCall result " + request.downloadHandler.text);

                    Debug.Log(request.downloadHandler.text);
                    CurrentUser = JsonConvert.DeserializeObject<User>(request.downloadHandler.text);
                    OnSuccess(CurrentUser);

                }
            }
        }


        

        private IEnumerator GetAccountBalance(Action<long> OnSuccess, Action<string> OnFailed, bool isRetry = false) {
            
            string url = API_URL + "/wallets/@me";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url)) {
                // request.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");
                request.SetRequestHeader("Authorization", "Bearer " + session.AccessToken);
                // Request and wait for the desired page.
                yield return request.SendWebRequest();


                if (request.isNetworkError || request.isHttpError) {
                    Debug.LogError(request.error);
                    Debug.LogError(request.downloadHandler.text);

                    if (!isRetry) {
                        //refresh access code and retry this call in case access code expired
                        StartCoroutine(GetAccessToken(delegate {
                            StartCoroutine(GetAccountBalance(OnSuccess, OnFailed, true));

                        }, delegate (string error) {
                            OnFailed(request.error);
                        }));
                    }
                    else {
                        Debug.LogError(request.error);
                        OnFailed(request.error);
                    }
                }
                else {
                    Debug.Log(request.downloadHandler.text);
                    JObject responseData = JsonConvert.DeserializeObject<JObject>(request.downloadHandler.text);
                    CurrentUser.Balance = responseData["balance"].ToObject<long>();
                    OnSuccess(CurrentUser.Balance);
                }
            }

        }


        
        public IEnumerator MakeTransaction(long coinsQuantity, string description, Action OnSuccess, Action<string> OnFailed) {
            
            Debug.Log("MakeTransaction");
            
            if (!session.DoesHaveRefreshToken) {
                Debug.LogError("Missing refresh token");
                OnFailed("Missing refresh token");
                yield break;
            }

            string url = API_URL + "/transactions";
            Debug.Log("Transaction url: " + url);
            
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("creditUser", coinsQuantity);
            postData.Add("description", description);

            string json = JsonConvert.SerializeObject(postData);


            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + session.AccessToken);
            request.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");
            
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError) {
                Debug.LogError(request.error);
                Debug.LogError(request.downloadHandler.text);
                    
                Error error = new Error(request.downloadHandler.text);
            
                if (error.ErrorDescription == Error.INVALID_TOKEN) {
                    //dispose invalid refresh token
                    session.RefreshToken = "";
                }
                    
                OnFailed?.Invoke(error.ErrorDescription);
            }
            else {
                Debug.Log(request.downloadHandler.text);

                GetAccountBalanceCall(delegate(long balance) {
                    OnSuccess?.Invoke();
                }, OnFailed);

            }
            
        }




        public void BuyQuarters() {

            string url = "https://www.poq.gg/buy";
            Application.OpenURL(url);
        }




        #endregion





	}







}