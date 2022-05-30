using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking;
using Newtonsoft.Json.Linq;
using QuartersSDK.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace QuartersSDK {
	public class Quarters : MonoBehaviour {
        
        public static Action<User> OnUserLoaded;
        public static Action<long> OnBalanceUpdated;
        public static Action OnSignOut;

        public static Quarters Instance;
        
        public Session session;
        public PCKE PCKE;
        [HideInInspector] public QuartersWebView QuartersWebView;
        
        public CurrencyConfig CurrencyConfig {
            get {
                return QuartersInit.Instance.CurrencyConfig;
            }
        }
        
        public List<Scope> DefaultScope = new List<Scope>() {
            Scope.identity,
            Scope.email,
            Scope.transactions,
            Scope.events,
            Scope.wallet
        };



        public string BASE_URL {
			get {
                Environment environment = QuartersInit.Instance.Environment;
                if (environment == Environment.production) return $"https://www.poq.gg";
                else if (environment == Environment.sandbox) return $"https://s2w-dev-firebase.herokuapp.com";
                return null;
            }
		}

  
		public string API_URL {
			get {
               
                return $"{BASE_URL}/api/v1";
			}
		}

        public string BUY_URL {
            get {
               
                return $"{BASE_URL}/buy";
            }
        }

        public static string URL_SCHEME = String.Empty;
        

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
            session = new Session();
            
            PCKE = new PCKE();
            URL_SCHEME = $"https://{QuartersInit.Instance.APP_UNIQUE_IDENTIFIER}.games.poq.gg"; ;
        }



        #region high level calls
        

        public void SignInWithQuarters(Action OnComplete, Action<string> OnError) {
            Session session = new Session();
            session.Scopes = DefaultScope;
            Quarters.Instance.Authorize(session.Scopes, delegate {
                OnComplete?.Invoke();
            }, OnError);

        }


        private void Authorize(List<Scope> scopes, Action OnSuccess, Action<string> OnError) {
            
            session = new Session();
            

            if (IsAuthorized) {

                StartCoroutine(GetAccessToken(delegate {
                    OnSuccess?.Invoke();
                }, delegate(string error) {
                    OnError?.Invoke(error);
                }));
                
                return;
            }

            
            string redirectSafeUrl = UnityWebRequest.EscapeURL(URL_SCHEME);

            string scopeString = "";
            foreach (Scope scope in scopes) {
                scopeString += scope.ToString();
                if (scopes.IndexOf(scope) != scopes.Count - 1) {
                    scopeString += " ";
                }
            }

            string url = BASE_URL + "/oauth2/authorize?response_type=code&client_id="
                                  + QuartersInit.Instance.APP_ID + "&redirect_uri="
                                  + redirectSafeUrl
                                  + $"&scope={UnityWebRequest.EscapeURL(scopeString)}"
                                  + $"&code_challenge_method=S256"
                                  + $"&code_challenge={PCKE.CodeChallenge()}";
                                
            Debug.Log(url);

            //web view authentication
            QuartersWebView.OpenURL(url, LinkType.External);
            QuartersWebView.OnDeepLink = delegate(QuartersLink link) {

                if (link.QueryString.ContainsKey("code")) {
                    string code = link.QueryString["code"];
                    StartCoroutine(GetRefreshToken(code, OnSuccess, OnError));
                    QuartersWebView.OnDeepLink = null;
                }
                else if (link.QueryString.ContainsKey("error")) {
                    OnError?.Invoke(link.QueryString["error"]);
                    QuartersWebView.OnDeepLink = null;
                }
            };

            
        }
        

    
        

        public void Deauthorize() {
            Session.Invalidate();
            this.session = null;
            CurrentUser = null;

            Debug.Log("Quarters user signed out");
            OnSignOut?.Invoke();
        }


        
		public void GetUserDetails(Action<User> OnSuccessDelegate, Action<string> OnFailedDelegate) {
            StartCoroutine(GetUserDetailsCall(OnSuccessDelegate, OnFailedDelegate));
		}

        public void GetAccountBalanceCall(Action<long> OnSuccess, Action<string> OnError) {
            StartCoroutine(GetAccountBalance(OnSuccess, OnError));
        }

        
        public void MakeTransactionCall(long coinsQuantity, string description, Action OnSuccess, Action<string> OnError) {
            StartCoroutine(MakeTransaction(coinsQuantity, description, OnSuccess, OnError));
        }


        

        #endregion

    
		// public void AuthorizationCodeReceived(string code) {
  //           
  //       }
        

        #region api calls

        private IEnumerator GetRefreshToken(string code, Action OnComplete, Action<string> OnError) {

            Debug.Log($"Get refresh token with code: {code}");

			WWWForm data = new WWWForm();
            data.AddField("code_verifier", PCKE.CodeVerifier);
            data.AddField("client_id", QuartersInit.Instance.APP_ID);
            data.AddField("grant_type", "authorization_code");
			data.AddField("code", code);
            data.AddField("redirect_uri", URL_SCHEME);
           
            
            string url = BASE_URL + "/api/oauth2/token";
            Debug.Log("GetRefreshToken url: " + url);

            using (UnityWebRequest request = UnityWebRequest.Post(url, data)) {
                yield return request.SendWebRequest();


                if (request.isNetworkError || request.isHttpError) {
                    Debug.LogError(request.error);
                    Debug.LogError(request.downloadHandler.text);
                    
                    OnError(request.error);
                }
                else {
                    Debug.Log(request.downloadHandler.text);
                    
                    Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
                    session.RefreshToken = responseData["refresh_token"];
                    session.AccessToken = responseData["access_token"];
                    session.SetScope(responseData["scope"]);

                    OnComplete();
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
            
            string url = BASE_URL + "/api/oauth2/token";
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


        private IEnumerator GetUserDetailsCall(Action<User> OnSuccess, Action<string> OnFailed, bool isRetry = false) {

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

            if (CurrentUser == null) {
                yield return GetUserDetailsCall(delegate(User user) { }, OnFailed);
            }
            
            
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
                    OnBalanceUpdated?.Invoke(CurrentUser.Balance);
                    OnSuccess(CurrentUser.Balance);
                }
            }

        }


        
        public IEnumerator MakeTransaction(long coinsQuantity, string description, Action OnSuccess, Action<string> OnFailed) {
            
            Debug.Log($"MakeTransaction with quantity: {coinsQuantity}");
            
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
            
            Debug.Log("Buy Quarters");
            
            string redirectSafeUrl = UnityWebRequest.EscapeURL(URL_SCHEME);
        
            string url = $"{BUY_URL}?redirect={redirectSafeUrl}";
            Debug.Log(url);

            QuartersWebView.OpenURL(url, LinkType.External);
            // QuartersWebView.OnDeepLink = DeepLink;
            
        }



        #endregion


        #region Deep linking


//         void OnApplicationFocus( bool focusStatus ){
//             if (focusStatus) {
//                 #if UNITY_ANDROID
// //                ProcessDeepLink(true);
//                 #endif
//             }
//         }

        
  
		// public void DeepLink (QuartersLink link) {
  //
		// 	Debug.Log("Deep link url: " + link.Uri);
  //
  //           if (!string.IsNullOrEmpty(link.Uri)) {
  //               
  //               
  //               
  //               ProcessDeepLink(link.QueryString);
  //           }
  //       }
  //       
        
        

        // private void ProcessDeepLink(Dictionary<string, string> urlParams) {
        //
        //     Debug.Log("ProcessDeepLink " + JsonConvert.SerializeObject(urlParams));
        //
        //     foreach (KeyValuePair<string,string> urlParam in urlParams) {
        //         Debug.Log(urlParam.Key + " : " + urlParam.Value);
        //     }
        //
        //     if (urlParams.ContainsKey("code")) {
        //         //string code = split[1];
        //         AuthorizationCodeReceived(urlParams["code"]);
        //     }
        //     
        // }

        #endregion

	}







}