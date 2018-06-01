using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Linq;


namespace QuartersSDK {
	public partial class Quarters : MonoBehaviour {

		public static Quarters Instance;
        public QuartersSession session;

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


        //Transfer
        public delegate void OnTransferRequestSuccessDelegate(TransferRequest transferRequest);
        public delegate void OnTransferRequestFailedDelegate(string error);

        public delegate void OnTransferSuccessDelegate(string transactionHash);
        public delegate void OnTransferFailedDelegate(string error);

        public List<TransferAPIRequest> currentTransferAPIRequests = new List<TransferAPIRequest>();

		public static string QUARTERS_URL {
			get {
				return "https://" + (QuartersInit.Instance.environment == Environment.development ? "dev." : "") + "pocketfulofquarters.com";
			}
		}

		public static string API_URL {
			get {
				return "https://api." + (QuartersInit.Instance.environment == Environment.development ? "dev." : "") + "pocketfulofquarters.com/v1";
			}
		}


        public string URL_SCHEME  {
            get {
                return Application.identifier + "://";
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
				Debug.Log("Quarters: User details loaded");
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

        public void Authorize(OnAuthorizationSuccessDelegate OnSuccessDelegate, OnAuthorizationFailedDelegate OnFailedDelegate, bool forceExternalBrowser = false) {

            if (!forceExternalBrowser && Application.platform == RuntimePlatform.WindowsEditor) {
                Debug.LogWarning("Quarters: WebView is not supported in Unity Editor on Windows. Falling back to forcing external browser. You can safely ignore this message");
                forceExternalBrowser = true;
            }


            session = new QuartersSession();

			this.OnAuthorizationSuccess = OnSuccessDelegate;
			this.OnAuthorizationFailed = OnFailedDelegate;

            if (IsAuthorized) {
                this.OnAuthorizationSuccess();
                return;
            }


			Debug.Log("Quarters: Authorize new session");

			if (OnAuthorizationStart != null) OnAuthorizationStart();

            if (Application.isEditor && forceExternalBrowser) {
				//spawn editor UI
				GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("QuartersEditor"));
                AuthorizeEditor();
			}
			else {
                //direct to the browser
                AuthorizeExternal();
			}

		}


        public void Deauthorize() {
            this.session.Invalidate();
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

        public void GetAccounts(OnAccountsSuccessDelegate OnSuccessDelegate, OnAccountsFailedDelegate OnFailedDelegate) {
            StartCoroutine(GetAccountsCall(OnSuccessDelegate, OnFailedDelegate));
        }

        public void GetAccountBalance(OnAccountBalanceSuccessDelegate OnSuccessDelegate, OnAccountBalanceFailedDelegate OnFailedDelegate) {
            StartCoroutine(GetAccountBalanceCall(OnSuccessDelegate, OnFailedDelegate));
        }



        public void CreateTransfer(TransferAPIRequest request) {

            StartCoroutine(CreateTransferRequestCall(request));
        }


        #endregion




        private void AuthorizeEditor() {
          
			string url =  QUARTERS_URL + "/access-token?app_id=" + QuartersInit.Instance.APP_ID + "&app_key=" + QuartersInit.Instance.APP_KEY;
            Application.OpenURL(url);

        }



        private void AuthorizeExternal(bool forceExternalBrowser = false) {

            Debug.Log("OAuth authorization");

			string url = QUARTERS_URL + "/oauth/authorize?response_type=code&client_id=" + QuartersInit.Instance.APP_ID + "&redirect_uri=" + URL_SCHEME + "&inline=true";
			Debug.Log(url);


            if (!forceExternalBrowser) {
                //web view authentication
                QuartersWebView.OpenURL(url);
                QuartersWebView.OnDeepLink = DeepLink;
            }
            else {
                //external authentication
			    Application.OpenURL(url);
            }

		}







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



        #region api calls


		public IEnumerator GetRefreshToken(string code) {

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("grant_type", "authorization_code");
			data.Add("code", code);
            data.Add("client_id", QuartersInit.Instance.APP_ID);
            data.Add("client_secret", QuartersInit.Instance.APP_KEY);

			string dataJson = JsonConvert.SerializeObject(data);
			Debug.Log(dataJson);
			byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(API_URL + "/oauth/token", dataBytes, AuthorizationHeader);
			Debug.Log(www.url);

			while (!www.isDone) yield return new WaitForEndOfFrame();

			if (!string.IsNullOrEmpty(www.error)) {
				Debug.LogError(www.error);

				OnAuthorizationFailed(www.error);
			}
			else {
				Debug.Log(www.text);

				Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(www.text);
				session.RefreshToken = responseData["refresh_token"];
                session.AccessToken = responseData["access_token"];

				OnAuthorizationSuccess();
			}
		}




        public IEnumerator GetAccessToken(Action OnSuccess, Action<string> OnFailed) {

            if (!session.DoesHaveRefreshToken) {
                Debug.LogError("Missing refresh token");
                yield break;
            }
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("grant_type", "refresh_token");
            data.Add("refresh_token", session.RefreshToken);
            data.Add("client_id", QuartersInit.Instance.APP_ID);
            data.Add("client_secret", QuartersInit.Instance.APP_KEY);

            string dataJson = JsonConvert.SerializeObject(data);
            Debug.Log(dataJson);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(API_URL + "/oauth/token", dataBytes, AuthorizationHeader);
            Debug.Log(www.url);

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
               
                string error = www.error;
                Debug.LogError(error);

                if (error == Error.Unauthorized) {
                    //dispose invalid refresh token
                    session.RefreshToken = "";
                }

                OnFailed(www.error);

                
            }
            else {
                Debug.Log(www.text);

                Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(www.text);
                //RefreshToken = responseData["refresh_token"];
                session.AccessToken = responseData["access_token"];

                OnSuccess();
                
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
                account.balance = JsonConvert.DeserializeObject<User.Account.Balance>(www.text);
                OnSucess(account.balance);

            }

        }




        private IEnumerator CreateTransferRequestCall(TransferAPIRequest request, bool forceExternalBrowser = false) {

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
                Debug.LogError(www.error);

                request.failedDelegate("Creating transfer failed: " + www.error);
            }
            else {
                Debug.Log(www.text);

                string response = www.text;
                Debug.Log("Response: " + response);

                TransferRequest transferRequest = new TransferRequest(response);

                request.requestId = transferRequest.id;
                Debug.Log("request id is: " + transferRequest.id);
                currentTransferAPIRequests.Add(request);

                //continue outh forward
                string url = QUARTERS_URL + "/requests/" + transferRequest.id + "?inline=true" + "&redirect_uri=" + URL_SCHEME;

                if (!forceExternalBrowser) {
                    //web view authentication
                    QuartersWebView.OpenURL(url);
                    QuartersWebView.OnDeepLink = DeepLink;
                }
                else {
                    //external authentication
                    Application.OpenURL(url);
                }
            }
        }




        #endregion


        #region Deep linking


        void OnApplicationFocus( bool focusStatus ){
            if (focusStatus) {
                #if UNITY_ANDROID
                ProcessDeepLink(true);
                #endif
            }
        }



		public void DeepLink (string url, bool isExternalBrowser) {

			Debug.Log("Deep link url: " + url);
            ProcessDeepLink(isExternalBrowser, url);
		}



        private void ProcessDeepLink(bool isExternalBrowser, string url = "") {

            string linkUrl = url;

            #if UNITY_ANDROID
           
            //overriden linkUrl if deep link comes from external browser, due to limitations of Android plugins implementation
            if (isExternalBrowser) {
                linkUrl = CustomUrlSchemeAndroid.GetLaunchedUrl(true);
                CustomUrlSchemeAndroid.ClearSavedData();
            }

            #endif


            #if UNITY_IOS
                //there is no link override on iOS
            #endif


            if (!string.IsNullOrEmpty(linkUrl)) {

                Debug.Log("Unity URL returned: " + linkUrl);

                Dictionary<string, string> urlParams = linkUrl.ParseURI();

                //blindcode as unable to test this without API update
                if (urlParams.ContainsKey("code")) {
                    //string code = split[1];
                    AuthorizationCodeReceived(urlParams["code"]);
                }
                else if (linkUrl.Contains("requestId=")) {

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

                        transferRequest.txId = urlParams["txId"];
                        Debug.Log("tx id:" + transferRequest.txId);

                        transferRequest.successDelegate(transferRequest.txId);
                    }

                    currentTransferAPIRequests.Remove(transferRequest);
                }
                else {
                    Debug.Log("NOT IMPLEMENTED URL: " + linkUrl);
                }
            }

        }

        #endregion

	}







}