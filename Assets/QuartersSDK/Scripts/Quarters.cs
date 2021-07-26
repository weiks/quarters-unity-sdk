using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Linq;
using ImaginationOverflow.UniversalDeepLinking;
using QuartersSDK.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace QuartersSDK {
	public partial class Quarters : MonoBehaviour {

        
        public static Action<User> OnUserLoaded;

		public static Quarters Instance;
        public Session session;
        public PCKE PCKE;
        
        
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
        
        public delegate void OnAwardSuccessDelegate(string transactionHash);
        public delegate void OnAwardFailedDelegate(string error);
        
        

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


        private Dictionary<string, string> AuthorizationHeader {
            get { 
                Dictionary<string, string> result = new Dictionary<string, string>();
                result.Add("Content-Type", "application/json;charset=UTF-8");
                return result;
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
            Debug.Log(PCKE.CodeVerifier);
            Debug.Log(PCKE.CodeChallenge());

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
            QuartersDeepLink.OpenURL(url);
            QuartersDeepLink.OnDeepLink = DeepLink;
            QuartersDeepLink.OnDeepLinkWebGL = DeepLinkWebGL;


            if (Application.isEditor) {
                AuthorizeEditorView.Instance.Show();
            }
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

            Debug.Log("Quarters user deauthorized");
        }




		public void GetUserDetails(OnUserDetailsSucessDelegate OnSuccessDelegate, OnUserDetailsFailedDelegate OnFailedDelegate) {
            StartCoroutine(GetUserDetailsCall(OnSuccessDelegate, OnFailedDelegate));
		}

        public void GetAccountBalanceCall(Action<long> OnSuccess, Action<string> OnError) {
            StartCoroutine(GetAccountBalance(OnSuccess, OnError));
        }




        public void CreateTransfer(TransferAPIRequest request) {
            // if (QuartersInit.Instance.useAutoapproval) {
            //     StartCoroutine(CreateAutoApprovedTransferCall(request));
            // }
            // else {
            //     //normal transfer
            //     StartCoroutine(CreateTransferRequestCall(request));
            // }

        }


        #endregion

        
        
        
        
        
        



		public void AuthorizationCodeReceived(string code) {

			Debug.Log("Quarters: Authorization code: " + code);
            
			StartCoroutine(GetRefreshToken(code));
        }


        //used only in Editor
        public void RefreshTokenReceived(string token) {

            Debug.Log("Quarters: Refresh token: " + token);
            session.RefreshToken = token;

            StartCoroutine(GetAccessToken(delegate {
                
                OnAuthorizationSuccess();

            }, delegate (string error) {
                
                OnAuthorizationFailed(error);

            }));

        }
        
        //  public void Award(int expectedAmount, OnAwardSuccessDelegate OnSuccessDelegate, OnAwardFailedDelegate OnFailedDelegate) {
        //     StartCoroutine(AwardCall(expectedAmount, OnSuccessDelegate, OnFailedDelegate));
        // }

        
        // private IEnumerator AwardCall(int expectedAward, OnAwardSuccessDelegate OnSucess, OnAwardFailedDelegate OnFailed) { 
        //     
        //     //pull user details if dont exist
        //     if (CurrentUser == null) {
        //         bool isUserDetailsDone = false;
        //         string getUserDetailsError = "";
        //
        //         StartCoroutine(GetUserDetailsCall(delegate (User user) {
        //             //user details loaded
        //             isUserDetailsDone = true;
        //
        //         }, delegate (string userDetailsError) {
        //             OnFailed("Getting user details failed: " + userDetailsError);
        //             isUserDetailsDone = true;
        //             getUserDetailsError = userDetailsError;
        //         }));
        //
        //         while (!isUserDetailsDone) yield return new WaitForEndOfFrame();
        //
        //         //error occured, break out of coroutine
        //         if (!string.IsNullOrEmpty(getUserDetailsError)) yield break;
        //     }
        //
        //
        //     string url = Quarters.Instance.API_URL + "/accounts/" + QuartersInit.Instance.ETHEREUM_ADDRESS + "/transfer";
        //     Debug.Log(url);
        //     
        //     Dictionary<string, object> data = new Dictionary<string, object>();
        //     data.Add("amount", expectedAward);
        //     data.Add("user", Quarters.Instance.CurrentUser.Id);
        //
        //     string dataJson = JsonConvert.SerializeObject(data);
        //     byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);
        //     
        //     Dictionary<string, string> header = new Dictionary<string, string>();
        //     header.Add("Authorization", QuartersInit.Instance.SERVER_API_TOKEN);
        //     header.Add("Content-Type", "application/json;charset=UTF-8");
        //     
        //     WWW www = new WWW(url, dataBytes, header);
        //
        //
        //     while (!www.isDone) {
        //         yield return new WaitForEndOfFrame();
        //     }
        //
        //     Debug.Log(www.text);
        //
        //     if (!string.IsNullOrEmpty(www.error)) {
        //         Debug.LogError(www.text);
        //         OnFailed(www.error + " " + www.text);
        //     }
        //     else {
        //         
        //         Hashtable ht = JsonConvert.DeserializeObject<Hashtable>(www.text);
        //
        //             if (ht.ContainsKey("txId")) {
        //                 
        //                 GetAccountBalance(delegate(User.Account.Balance balance) {
        //                     
        //                     OnSucess((string)ht["txId"]);
        //                 }, delegate(string error) {
        //                     OnFailed(error);
        //                 });
        //                 
        //             }
        //             else {
        //                 Debug.Log(JsonConvert.SerializeObject(www.text));
        //                 OnFailed("Unknown error");
        //             }
        //     }
        //
        //
        // }



        #region api calls

        



		public IEnumerator GetRefreshToken(string code) {

            Debug.Log($"Get refresh token with code: {code}");

			WWWForm data = new WWWForm();
            data.AddField("code_verifier", PCKE.CodeVerifier);
            data.AddField("client_id", QuartersInit.Instance.APP_ID);
            data.AddField("grant_type", "authorization_code");
			data.AddField("code", code);
            data.AddField("redirect_uri", URL_SCHEME);
           
            
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
            
            string url = API_URL + "wallets/@me";

            WWW www = new WWW(url, null, AuthorizationHeader);
            yield return www;

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
                
                if (!isRetry) {
                    //refresh access code and retry this call in case access code expired
                    StartCoroutine(GetAccessToken(delegate {
                        StartCoroutine(GetAccountBalance(OnSuccess, OnFailed, true));

                    }, delegate (string error) {
                        OnFailed(www.error);
                    }));
                } 
                else {
                    Debug.LogError(www.error);
                    OnFailed(www.error);
                }
            }
            else {

                Debug.Log(www.text);
           

            }

        }




      


        private void AddOrSwapAPITransferRequest(TransferAPIRequest request) {

            for (int i = 0; i < currentTransferAPIRequests.Count; i++) {

                if (currentTransferAPIRequests[i].requestId == request.requestId) {
                    currentTransferAPIRequests[i] = request;
                    return;
                }
                
            }
            
            currentTransferAPIRequests.Add(request);
            
        }





        private IEnumerator CreateTransferRequestCall(TransferAPIRequest request, bool isRetry = false, bool forceExternalBrowser = false) {

            if (Application.isEditor && forceExternalBrowser) Debug.LogWarning("Quarters: Transfers with external browser arent supported in Unity editor");

            Debug.Log("CreateTransferRequestCall");

            Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
            headers.Add("Authorization", "Bearer " + session.AccessToken);


            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("tokens", request.tokens);
            if (!string.IsNullOrEmpty(request.description)) data.Add("description", request.description);
            data.Add("app_id", QuartersInit.Instance.APP_ID);




            string dataJson = JsonConvert.SerializeObject(data);
            Debug.Log(dataJson);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(API_URL + "/requests", dataBytes, headers);
            Debug.Log(www.url);

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
                Debug.Log(www.error);

                if (www.error == Error.UNAUTHORIZED_ERROR && !isRetry) {
                    //token expired
                    StartCoroutine(GetAccessToken(delegate {
                        
                        StartCoroutine(CreateTransferRequestCall(request, true, forceExternalBrowser));
                        
                    }, delegate(string error) {
                        request.failedDelegate(error);
                    }));
                }
                else {
                    request.failedDelegate("Creating transfer failed: " + www.error);
                }
          
                
               
            }
            else {
                Debug.Log(www.text);

                string response = www.text;
                Debug.Log("Response: " + response);

                TransferRequest transferRequest = new TransferRequest(response);

                request.requestId = transferRequest.id;
                Debug.Log("request id is: " + transferRequest.id);
                AddOrSwapAPITransferRequest(request);

                //continue outh forward
                string url = BASE_URL + "/requests/" + transferRequest.id + "?inline=true" + "&redirect_uri=" + URL_SCHEME;

                // if (session.IsGuestSession) {
                //     url += "&firebase_token=" + session.GuestFirebaseToken;
                // }

                Debug.Log("Transfer authorization url: " + url);

                if (!forceExternalBrowser) {
                    //web view authentication
                    QuartersDeepLink.OpenURL(url);
                    QuartersDeepLink.OnDeepLink = DeepLink;
                    QuartersDeepLink.OnDeepLinkWebGL = DeepLinkWebGL;
                    QuartersDeepLink.OnCancelled += delegate {
                        request.failedDelegate("User canceled");
                        QuartersDeepLink.OnCancelled = null;
                    };
                }
                else {
                    //external authentication
                    Application.OpenURL(url);
                }
            }
        }




        // private IEnumerator CreateAutoApprovedTransferCall(TransferAPIRequest request, bool isRetry = false) {
        //
        //     Debug.Log("CreateAutoApprovedTransfer");
        //     
        //     if (string.IsNullOrEmpty(session.AccessToken)) {
        //         yield return StartCoroutine(GetAccessToken(null, null));
        //     }
        //     
        //
        //     Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
        //     headers.Add("Authorization", "Bearer " + session.AccessToken);
        //     
        //     Debug.Log("Headers: " + JsonConvert.SerializeObject(headers));
        //
        //
        //     Dictionary<string, object> data = new Dictionary<string, object>();
        //     data.Add("tokens", request.tokens);
        //     if (!string.IsNullOrEmpty(request.description)) data.Add("description", request.description);
        //     data.Add("app_id", QuartersInit.Instance.APP_ID);
        //
        //
        //     string dataJson = JsonConvert.SerializeObject(data);
        //     Debug.Log(dataJson);
        //     byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);
        //
        //
        //     WWW www = new WWW(API_URL + "/requests", dataBytes, headers);
        //     Debug.Log(www.url);
        //
        //     while (!www.isDone) yield return new WaitForEndOfFrame();
        //
        //     
        //     if (!string.IsNullOrEmpty(www.error)) {
        //         Debug.Log(www.error);
        //
        //         if (www.error == Error.UNAUTHORIZED_ERROR && !isRetry) {
        //         
        //             //token expired
        //             StartCoroutine(GetAccessToken(delegate {
        //                 
        //                 StartCoroutine(CreateAutoApprovedTransferCall(request, true));
        //                 
        //             }, delegate(string error) {
        //                 request.failedDelegate(error);
        //             }));
        //           
        //         }
        //         else {
        //             //failed, fallback to normal request
        //             StartCoroutine(CreateTransferRequestCall(request, true));
        //         }
        //     }
        //     else {
        //         Debug.Log(www.text);
        //
        //         string response = www.text;
        //         Debug.Log("Response: " + response);
        //
        //         TransferRequest transferRequest = new TransferRequest(response);
        //
        //         request.requestId = transferRequest.id;
        //         Debug.Log("request id is: " + transferRequest.id);
        //         AddOrSwapAPITransferRequest(request);
        //
        //         StartCoroutine(AutoApproveTransfer(request));
        //
        //
        //     }
        // }





        // private IEnumerator AutoApproveTransfer(TransferAPIRequest request) {
        //     
        //     
        //     bool areAccountsDone = false;
        //     string accountsLoadingError = "";
        //     
        //     if (CurrentUser == null || CurrentUser.accounts.Count == 0) {
        //         Quarters.Instance.GetAccounts(delegate (List<User.Account> accounts) {
        //             //accounts loaded
        //             areAccountsDone = true;
        //
        //         }, delegate (string getAccountsError) {
        //             request.failedDelegate("Getting user accounts failed: " + getAccountsError);
        //             areAccountsDone = true;
        //             accountsLoadingError = getAccountsError;
        //         });
        //
        //         while (!areAccountsDone) yield return new WaitForEndOfFrame();
        //
        //         //error occured, break out of coroutine
        //         if (!string.IsNullOrEmpty(accountsLoadingError)) yield break;
        //     }
        //
        //     if (CurrentUser.accounts.Count < 1) {
        //         request.failedDelegate("User account not loaded");
        //         yield break;
        //     }
        //
        //     Debug.Log("AutoApproveTransfer");
        //
        //     Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
        //     headers.Add("Authorization", "Bearer " + QuartersInit.Instance.SERVER_API_TOKEN);
        //
        //
        //     Dictionary<string, object> data = new Dictionary<string, object>();
        //     data.Add("clientId", QuartersInit.Instance.APP_ID);
        //     data.Add("userId", CurrentUser.Id);
        //     data.Add("address", CurrentUser.accounts[0].address);
        //
        //
        //     string dataJson = JsonConvert.SerializeObject(data);
        //
        //     byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);
        //
        //
        //     WWW www = new WWW(API_URL + "/requests/" + request.requestId + "/autoApprove", dataBytes, headers);
        //     Debug.Log(JsonConvert.SerializeObject(headers));
        //     Debug.Log(www.url);
        //     Debug.Log(dataJson);
        //
        //     while (!www.isDone) yield return new WaitForEndOfFrame();
        //
        //     if (!string.IsNullOrEmpty(www.error)) {
        //         Debug.Log(www.text);
        //         if (www.error.StartsWith("40")) {
        //
        //             //bad request check for reason
        //             if (www.error.StartsWith("400") && !string.IsNullOrEmpty(www.text)) {
        //                 //bad request, possibly out of coins
        //                 Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(www.text);
        //                 if (responseData.ContainsKey("message")) {
        //                     request.failedDelegate(responseData["message"]);
        //                 }
        //                 else {
        //                     StartCoroutine(CreateTransferRequestCall(request));
        //                 }
        //             }
        //             else {
        //                 //fallback to normal transfer automatically
        //                 StartCoroutine(CreateTransferRequestCall(request));
        //             }
        //         }
        //         else {
        //             Debug.LogError(www.error);
        //             request.failedDelegate(www.error);
        //         }
        //
        //     }
        //     else {
        //         Debug.Log(www.text);
        //
        //         string response = www.text;
        //         Debug.Log("Autoapprove response: " + response);
        //
        //         Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string,string>>(response);
        //
        //         request.txId = responseData["txId"];
        //
        //         Debug.Log("Autoapproved request txId is: " + request.txId);
        //         
        //         GetAccountBalance(delegate(User.Account.Balance balance) {
        //                     
        //             request.successDelegate(request.txId);
        //             
        //         }, delegate(string error) {
        //             request.failedDelegate(error);
        //         });
        //     }
        // }






        #endregion


        #region Deep linking


        void OnApplicationFocus( bool focusStatus ){
            if (focusStatus) {
                #if UNITY_ANDROID
//                ProcessDeepLink(true);
                #endif
            }
        }

        
  
        
		public void DeepLink (LinkActivation linkActivation) {

			Debug.Log("Deep link url: " + linkActivation.Uri);

            if (!string.IsNullOrEmpty(linkActivation.Uri)) {
                ProcessDeepLink(linkActivation.QueryString);
            }
        }

        
        
        

        public void DeepLinkWebGL(Dictionary<string, string> urlParams) {
            ProcessDeepLink(urlParams);
        }


        private void ProcessDeepLink(Dictionary<string, string> urlParams) {

            Debug.Log("ProcessDeepLink " + JsonConvert.SerializeObject(urlParams));

            foreach (KeyValuePair<string,string> urlParam in urlParams) {
                Debug.Log(urlParam.Key + " : " + urlParam.Value);
            }

            if (urlParams.ContainsKey("code")) {
                //string code = split[1];
                AuthorizationCodeReceived(urlParams["code"]);
            }
            else if (urlParams.ContainsKey("requestId")) {

                string transferId = urlParams["requestId"];

                foreach (TransferAPIRequest r in currentTransferAPIRequests) {
                    Debug.Log("Current requests id: " + r.requestId);
                }

                //get request from ongoing
                TransferAPIRequest transferRequest = currentTransferAPIRequests.Find(t => t.requestId == transferId);
                if (transferRequest == null) {
                    Debug.LogError("Transfer id is invalid: " + transferId);
                    transferRequest.failedDelegate("Invalid transfer id: " + transferId);
                }

                if (urlParams.ContainsKey("error")) {
                    //all requests are validated positivelly currently
                    transferRequest.failedDelegate(urlParams["error"]);
                }
                else {

                    
                    GetAccountBalance(delegate(long balance) {
                        transferRequest.txId = urlParams["txId"];
                        Debug.Log("tx id:" + transferRequest.txId);

                        transferRequest.successDelegate(transferRequest.txId);
                    
                    }, delegate(string error) {
                        transferRequest.failedDelegate(error);
                    });
                }

                currentTransferAPIRequests.Remove(transferRequest);
            }
            else if (urlParams.ContainsKey("cancel")) {
                if (urlParams["cancel"] == "true") {
                    
                    Debug.Log("User canceled deep link");
                    Debug.Log($"currentTransferAPIRequests count {currentTransferAPIRequests.Count.ToString()}");
                    if (currentTransferAPIRequests.Count > 0) {
                        currentTransferAPIRequests[0].failedDelegate("User canceled");
                    }
                }
            }



        }

        #endregion

	}







}