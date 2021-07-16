using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Linq;
using ImaginationOverflow.UniversalDeepLinking;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace QuartersSDK {
	public partial class Quarters : MonoBehaviour {

        
        public static Action<User> OnUserLoaded;

		public static Quarters Instance;
        public Session session;

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

        public delegate void OnAccountsSuccessDelegate(List<User.Account> accounts);
        public delegate void OnAccountsFailedDelegate(string error);

        public delegate void OnAccountBalanceSuccessDelegate(User.Account.Balance balance);
        public delegate void OnAccountBalanceFailedDelegate(string error);

        public delegate void OnAccounRewardSuccessDelegate(User.Account.Reward reward);
        public delegate void OnAccountRewardFailedDelegate(string error);

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
                Environment environment = QuartersInit.Instance.environment;
                if (environment == Environment.production) return $"https://api.{CurrencyConfig.APIBaseUrl}/v1";
                else if (environment == Environment.development) return $"https://api.dev.{CurrencyConfig.APIBaseUrl}/v1";
                else if (environment == Environment.sandbox) return $"https://api.sandbox.{CurrencyConfig.APIBaseUrl}/v1";
                return null;
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
		}



        #region high level calls

        public void AuthorizeGuest(OnAuthorizationSuccessDelegate OnSuccessDelegate, OnAuthorizationFailedDelegate OnFailedDelegate) {

            session = new Session();

            if (!string.IsNullOrEmpty(session.RefreshToken)) {
                Deauthorize();
                // Debug.LogError("Authorization error. Registered user session exist. Use Authorize User call instead, or Deauthorize Coinforge user first");
            }

            this.OnAuthorizationSuccess = OnSuccessDelegate;
            this.OnAuthorizationFailed = OnFailedDelegate;

            if (IsAuthorized) {
                this.OnAuthorizationSuccess();
                return;
            }

            if (OnAuthorizationStart != null) OnAuthorizationStart();

            //create new guest account
            StartCoroutine(CreateNewGuestUser());

        }




        public void Authorize(OnAuthorizationSuccessDelegate OnSuccessDelegate, OnAuthorizationFailedDelegate OnFailedDelegate) {
            
            session = new Session();

			this.OnAuthorizationSuccess = OnSuccessDelegate;
			this.OnAuthorizationFailed = OnFailedDelegate;

            if (IsAuthorized && !session.IsGuestSession) {
                this.OnAuthorizationSuccess();
                return;
            }

			if (OnAuthorizationStart != null) OnAuthorizationStart();


            Debug.Log("OAuth authorization");
            
            string redirectSafeUrl = UnityWebRequest.EscapeURL(URL_SCHEME);;

            string url = BASE_URL + "/oauth2/authorize?response_type=code&client_id=" + QuartersInit.Instance.APP_ID + "&redirect_uri=" + redirectSafeUrl + "&scope=email";
            Debug.Log(url);

            //web view authentication
            QuartersDeepLink.OpenURL(url);
            QuartersDeepLink.OnDeepLink = DeepLink;
            QuartersDeepLink.OnDeepLinkWebGL = DeepLinkWebGL;
        }
        

    



        public void SignUp(OnAuthorizationSuccessDelegate OnSuccessDelegate, OnAuthorizationFailedDelegate OnFailedDelegate) {

            string url =  BASE_URL + "/guest?token=" + session.GuestToken + "&redirect_uri=" + URL_SCHEME + "&inline=trueresponse_type=code&client_id=" + QuartersInit.Instance.APP_ID;

            this.OnAuthorizationSuccess = OnSuccessDelegate;
            this.OnAuthorizationFailed = OnFailedDelegate;


            //web view authentication
            QuartersDeepLink.OpenURL(url);
            QuartersDeepLink.OnDeepLink = DeepLink;
            QuartersDeepLink.OnDeepLinkWebGL = DeepLinkWebGL;
            QuartersDeepLink.OnCancelled += delegate {
                //webview was closed
                OnFailedDelegate("User canceled");
                QuartersDeepLink.OnCancelled = null;
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

            Debug.Log("Quarters user deauthorized");
        }




		public void GetUserDetails(OnUserDetailsSucessDelegate OnSuccessDelegate, OnUserDetailsFailedDelegate OnFailedDelegate) {
            StartCoroutine(GetUserDetailsCall(OnSuccessDelegate, OnFailedDelegate));
		}

        public void GetAccounts(OnAccountsSuccessDelegate OnSuccessDelegate, OnAccountsFailedDelegate OnFailedDelegate) {
            StartCoroutine(GetAccountsCall(OnSuccessDelegate, OnFailedDelegate));
        }

        public void GetAccountBalance(OnAccountBalanceSuccessDelegate OnSuccessDelegate, OnAccountBalanceFailedDelegate OnFailedDelegate) {
            StartCoroutine(GetAccountBalanceCall(OnSuccessDelegate, OnFailedDelegate));
        }

        public void GetAccountReward(OnAccounRewardSuccessDelegate OnSuccessDelegate, OnAccountRewardFailedDelegate OnFailedDelegate) {
            StartCoroutine(GetAccountRewardCall(OnSuccessDelegate, OnFailedDelegate));
        }



        public void CreateTransfer(TransferAPIRequest request) {
            if (QuartersInit.Instance.useAutoapproval) {
                StartCoroutine(CreateAutoApprovedTransferCall(request));
            }
            else {
                //normal transfer
                StartCoroutine(CreateTransferRequestCall(request));
            }

        }


        #endregion

        
        
        
        
        
        



		public void AuthorizationCodeReceived(string code) {

			Debug.Log("Quarters: Authorization code: " + code);

            if (session.IsGuestSession) {
                //real user authorisation, conversion from guest to real user. Invalidate and destroy guest token
                session.InvalidateGuestSession();
            }


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
        
         public void Award(int expectedAmount, OnAwardSuccessDelegate OnSuccessDelegate, OnAwardFailedDelegate OnFailedDelegate) {
            StartCoroutine(AwardCall(expectedAmount, OnSuccessDelegate, OnFailedDelegate));
        }

        
        private IEnumerator AwardCall(int expectedAward, OnAwardSuccessDelegate OnSucess, OnAwardFailedDelegate OnFailed) { 
            
            //pull user details if dont exist
            if (CurrentUser == null) {
                bool isUserDetailsDone = false;
                string getUserDetailsError = "";

                StartCoroutine(GetUserDetailsCall(delegate (User user) {
                    //user details loaded
                    isUserDetailsDone = true;

                }, delegate (string userDetailsError) {
                    OnFailed("Getting user details failed: " + userDetailsError);
                    isUserDetailsDone = true;
                    getUserDetailsError = userDetailsError;
                }));

                while (!isUserDetailsDone) yield return new WaitForEndOfFrame();

                //error occured, break out of coroutine
                if (!string.IsNullOrEmpty(getUserDetailsError)) yield break;
            }


            string url = Quarters.Instance.API_URL + "/accounts/" + QuartersInit.Instance.ETHEREUM_ADDRESS + "/transfer";
            Debug.Log(url);
            
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("amount", expectedAward);
            data.Add("user", Quarters.Instance.CurrentUser.userId);

            string dataJson = JsonConvert.SerializeObject(data);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);
            
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", QuartersInit.Instance.SERVER_API_TOKEN);
            header.Add("Content-Type", "application/json;charset=UTF-8");
            
            WWW www = new WWW(url, dataBytes, header);


            while (!www.isDone) {
                yield return new WaitForEndOfFrame();
            }

            Debug.Log(www.text);

            if (!string.IsNullOrEmpty(www.error)) {
                Debug.LogError(www.text);
                OnFailed(www.error + " " + www.text);
            }
            else {
                
                Hashtable ht = JsonConvert.DeserializeObject<Hashtable>(www.text);

                    if (ht.ContainsKey("txId")) {
                        
                        GetAccountBalance(delegate(User.Account.Balance balance) {
                            
                            OnSucess((string)ht["txId"]);
                        }, delegate(string error) {
                            OnFailed(error);
                        });
                        
                    }
                    else {
                        Debug.Log(JsonConvert.SerializeObject(www.text));
                        OnFailed("Unknown error");
                    }
            }
 

        }



        #region api calls


        public IEnumerator CreateNewGuestUser() {

            Debug.Log("Create new guest account");

            Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
            headers.Add("Authorization", "Bearer " + QuartersInit.Instance.SERVER_API_TOKEN);

            Dictionary<string, object> data = new Dictionary<string, object>();
            string dataJson = JsonConvert.SerializeObject(data);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(API_URL + "/new-guest", dataBytes, headers);
            Debug.Log(www.url);

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
                Debug.LogError("Create new guest account failed: " + www.error);
                OnAuthorizationFailed(www.error);

            }
            else {
                Debug.Log(www.text);

                //deserialize
                Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(www.text);
                session.GuestToken = responseData["access_token"];
                session.GuestFirebaseToken = responseData["firebase_token"];
                OnAuthorizationSuccess();

            }

        }





		public IEnumerator GetRefreshToken(string code) {

            Debug.Log("Get refresh token");

			WWWForm data = new WWWForm();
			data.AddField("grant_type", "authorization_code");
			data.AddField("code", code);
            data.AddField("client_id", QuartersInit.Instance.APP_ID);
            data.AddField("client_secret", QuartersInit.Instance.APP_KEY);
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

                    OnAuthorizationSuccess();
                }
            }
        }
        
        


        public IEnumerator GetAccessToken(Action OnSuccess, Action<string> OnFailed) {
            
            Debug.Log("Get Access token");
            
            //skip call for guest users
            if (session.IsGuestSession) {
                OnSuccess();
                yield break;
            }

            if (!session.DoesHaveRefreshToken) {
                Debug.LogError("Missing refresh token");
                OnFailed("Missing refresh token");
                yield break;
            }
            
            
            WWWForm data = new WWWForm();
            data.AddField("grant_type", "refresh_token");
            data.AddField("client_id", QuartersInit.Instance.APP_ID);
            data.AddField("client_secret", QuartersInit.Instance.APP_KEY);
            data.AddField("redirect_uri", URL_SCHEME);
            
            string url = BASE_URL + "/oauth2/token";
            Debug.Log("GetAccessToken url: " + url);
            
            
            using (UnityWebRequest request = UnityWebRequest.Post(url, data)) {
                yield return request.SendWebRequest();


                if (request.isNetworkError || request.isHttpError) {
                    Debug.LogError(request.error);
                    Debug.LogError(request.downloadHandler.text);

                    if (request.error == Error.UNAUTHORIZED_ERROR) {
                        //dispose invalid refresh token
                        session.RefreshToken = "";
                    }
                    
                    OnFailed?.Invoke(request.error);
                }
                else {
                    Debug.Log(request.downloadHandler.text);
                    
                    Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.downloadHandler.text);
                    session.AccessToken = responseData["access_token"];
                    OnSuccess?.Invoke();
                }
            }
            
            
        }



        private IEnumerator GetUserDetailsCall(OnUserDetailsSucessDelegate OnSucess, OnUserDetailsFailedDelegate OnFailed, bool isRetry = false) {

            Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
            headers.Add("Authorization", "Bearer " + session.AccessToken);

            WWW www = new WWW(API_URL + "/me", null, headers);
			yield return www;

			while (!www.isDone) yield return new WaitForEndOfFrame();

			if (!string.IsNullOrEmpty(www.error)) {
				
                if (!isRetry) {
                    //refresh access code and retry this call in case access code expired
                    StartCoroutine(GetAccessToken(delegate {
                       
                        StartCoroutine(GetUserDetailsCall(OnSucess, OnFailed, true));

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
				CurrentUser = JsonConvert.DeserializeObject<User>(www.text);
                OnSucess(CurrentUser);
			
			}
		}





        private IEnumerator GetAccountsCall(OnAccountsSuccessDelegate OnSucess, OnAccountsFailedDelegate OnFailed, bool isRetry = false) {

            Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
            headers.Add("Authorization", "Bearer " + session.AccessToken);

            //pull user details if dont exist
            if (CurrentUser == null) {
                bool isUserDetailsDone = false;
                string getUserDetailsError = "";

                StartCoroutine(GetUserDetailsCall(delegate (User user) {
                    //user details loaded
                    isUserDetailsDone = true;

                }, delegate (string userDetailsError) {
                    OnFailed("Getting user details failed: " + userDetailsError);
                    isUserDetailsDone = true;
                    getUserDetailsError = userDetailsError;
                }));

                while (!isUserDetailsDone) yield return new WaitForEndOfFrame();

                //error occured, break out of coroutine
                if (!string.IsNullOrEmpty(getUserDetailsError)) yield break;
            }

            WWW www = new WWW(API_URL + "/accounts", null, headers);
            yield return www;

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {

                if (!isRetry) {
                    //refresh access code and retry this call in case access code expired
                    StartCoroutine(GetAccessToken(delegate {

                        StartCoroutine(GetAccountsCall(OnSucess, OnFailed, true));

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
                CurrentUser.accounts = JsonConvert.DeserializeObject<List<User.Account>>(www.text);
                CurrentUser.OnAccountsLoaded?.Invoke();
                OnSucess(CurrentUser.accounts);

            }
        }




        private IEnumerator GetAccountBalanceCall(OnAccountBalanceSuccessDelegate OnSucess, OnAccountBalanceFailedDelegate OnFailed, bool isRetry = false) {

            bool areAccountsDone = false;
            string accountsLoadingError = "";

            if (CurrentUser == null || CurrentUser.accounts.Count == 0) {
                Quarters.Instance.GetAccounts(delegate (List<User.Account> accounts) {
                    //accounts loaded
                    areAccountsDone = true;

                }, delegate (string getAccountsError) {
                    OnFailed("Getting user accounts failed: " + getAccountsError);
                    areAccountsDone = true;
                    accountsLoadingError = getAccountsError;
                });

                while (!areAccountsDone) yield return new WaitForEndOfFrame();

                //error occured, break out of coroutine
                if (!string.IsNullOrEmpty(accountsLoadingError)) yield break;
            }

            if (CurrentUser.accounts.Count < 1) {
                OnFailed("User account not loaded");
                yield break;
            }

            User.Account account = CurrentUser.accounts[0];

            string url = API_URL + "/accounts/" + account.address + "/balance";

            WWW www = new WWW(url, null, AuthorizationHeader);
            yield return www;

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
                
                if (!isRetry) {
                    //refresh access code and retry this call in case access code expired
                    StartCoroutine(GetAccessToken(delegate {

                        StartCoroutine(GetAccountBalanceCall(OnSucess, OnFailed, true));

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
                account.CurrentBalance = JsonConvert.DeserializeObject<User.Account.Balance>(www.text);
                OnSucess(account.CurrentBalance);

            }

        }




        private IEnumerator GetAccountRewardCall(OnAccounRewardSuccessDelegate OnSucess, OnAccountRewardFailedDelegate OnFailed, bool isRetry = false) {

            bool areAccountsDone = false;
            string accountsLoadingError = "";

            if (CurrentUser == null || CurrentUser.accounts.Count == 0) {
                Quarters.Instance.GetAccounts(delegate (List<User.Account> accounts) {
                    //accounts loaded
                    areAccountsDone = true;

                }, delegate (string getAccountsError) {
                    OnFailed("Getting user accounts failed: " + getAccountsError);
                    areAccountsDone = true;
                    accountsLoadingError = getAccountsError;
                });

                while (!areAccountsDone) yield return new WaitForEndOfFrame();

                //error occured, break out of coroutine
                if (!string.IsNullOrEmpty(accountsLoadingError)) yield break;
            }

            if (CurrentUser.accounts.Count < 1) {
                OnFailed("User account not loaded");
                yield break;
            }

            User.Account account = CurrentUser.accounts[0];

            string url = API_URL + "/accounts/" + account.address + "/rewards";

            WWW www = new WWW(url, null, AuthorizationHeader);
            yield return www;

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {

                if (!isRetry) {
                    //refresh access code and retry this call in case access code expired
                    StartCoroutine(GetAccessToken(delegate {

                        StartCoroutine(GetAccountRewardCall(OnSucess, OnFailed, true));

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
                account.CurrentReward = JsonConvert.DeserializeObject<User.Account.Reward>(www.text);
                OnSucess(account.CurrentReward);

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

                if (session.IsGuestSession) {
                    url += "&firebase_token=" + session.GuestFirebaseToken;
                }

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




        private IEnumerator CreateAutoApprovedTransferCall(TransferAPIRequest request, bool isRetry = false) {
        
            Debug.Log("CreateAutoApprovedTransfer");
            
            if (string.IsNullOrEmpty(session.AccessToken)) {
                yield return StartCoroutine(GetAccessToken(null, null));
            }
            

            Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
            headers.Add("Authorization", "Bearer " + session.AccessToken);
            
            Debug.Log("Headers: " + JsonConvert.SerializeObject(headers));


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
                        
                        StartCoroutine(CreateAutoApprovedTransferCall(request, true));
                        
                    }, delegate(string error) {
                        request.failedDelegate(error);
                    }));
                  
                }
                else {
                    //failed, fallback to normal request
                    StartCoroutine(CreateTransferRequestCall(request, true));
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

                StartCoroutine(AutoApproveTransfer(request));


            }
        }





        private IEnumerator AutoApproveTransfer(TransferAPIRequest request) {
            
            
            bool areAccountsDone = false;
            string accountsLoadingError = "";
            
            if (CurrentUser == null || CurrentUser.accounts.Count == 0) {
                Quarters.Instance.GetAccounts(delegate (List<User.Account> accounts) {
                    //accounts loaded
                    areAccountsDone = true;

                }, delegate (string getAccountsError) {
                    request.failedDelegate("Getting user accounts failed: " + getAccountsError);
                    areAccountsDone = true;
                    accountsLoadingError = getAccountsError;
                });

                while (!areAccountsDone) yield return new WaitForEndOfFrame();

                //error occured, break out of coroutine
                if (!string.IsNullOrEmpty(accountsLoadingError)) yield break;
            }

            if (CurrentUser.accounts.Count < 1) {
                request.failedDelegate("User account not loaded");
                yield break;
            }

            Debug.Log("AutoApproveTransfer");

            Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
            headers.Add("Authorization", "Bearer " + QuartersInit.Instance.SERVER_API_TOKEN);


            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("clientId", QuartersInit.Instance.APP_ID);
            data.Add("userId", CurrentUser.userId);
            data.Add("address", CurrentUser.accounts[0].address);


            string dataJson = JsonConvert.SerializeObject(data);

            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(API_URL + "/requests/" + request.requestId + "/autoApprove", dataBytes, headers);
            Debug.Log(JsonConvert.SerializeObject(headers));
            Debug.Log(www.url);
            Debug.Log(dataJson);

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
                Debug.Log(www.text);
                if (www.error.StartsWith("40")) {

                    //bad request check for reason
                    if (www.error.StartsWith("400") && !string.IsNullOrEmpty(www.text)) {
                        //bad request, possibly out of coins
                        Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(www.text);
                        if (responseData.ContainsKey("message")) {
                            request.failedDelegate(responseData["message"]);
                        }
                        else {
                            StartCoroutine(CreateTransferRequestCall(request));
                        }
                    }
                    else {
                        //fallback to normal transfer automatically
                        StartCoroutine(CreateTransferRequestCall(request));
                    }
                }
                else {
                    Debug.LogError(www.error);
                    request.failedDelegate(www.error);
                }

            }
            else {
                Debug.Log(www.text);

                string response = www.text;
                Debug.Log("Autoapprove response: " + response);

                Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string,string>>(response);

                request.txId = responseData["txId"];

                Debug.Log("Autoapproved request txId is: " + request.txId);
                
                GetAccountBalance(delegate(User.Account.Balance balance) {
                            
                    request.successDelegate(request.txId);
                    
                }, delegate(string error) {
                    request.failedDelegate(error);
                });
            }
        }






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

                    
                    GetAccountBalance(delegate(User.Account.Balance balance) {
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