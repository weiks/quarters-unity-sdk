using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Linq;


namespace Quarters {
	public class Quarters : MonoBehaviour {

		public static Quarters Instance;

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

		public const string QUARTERS_URL = "https://pocketfulofquarters.com";
		public const string API_URL = "https://api.dev.pocketfulofquarters.com/v1/";

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


		public bool DoesHaveRefreshToken {
			get {
				return !string.IsNullOrEmpty(refreshToken);
			}
		}

		public bool DoesHaveAccessToken {
			get {
				return !string.IsNullOrEmpty(accessToken);
			}
		}


		#region tokens

		private string refreshToken = "";
		public string RefreshToken {
			get {
				return refreshToken;
			}
			set {
				refreshToken = value;
			}
		}


		private string accessToken = "";
		public string AccessToken {
			get {
				return accessToken;
			}
			set {
				accessToken = value;
			}
		}

		#endregion


		public bool IsAuthorized {
			get {
				return DoesHaveRefreshToken;
			}
		}



		public void Init() {
			Instance = this;
		}



        #region high level calls

		public void Authorize(OnAuthorizationSuccessDelegate OnSuccessDelegate, OnAuthorizationFailedDelegate OnFailedDelegate) {

			this.OnAuthorizationSuccess = OnSuccessDelegate;
			this.OnAuthorizationFailed = OnFailedDelegate;

			Debug.Log("Quarters: Authorize");

			if (OnAuthorizationStart != null) OnAuthorizationStart();

			if (Application.isEditor) {
				//spawn editor UI
				GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("QuartersEditor"));
                AuthorizeEditor();
			}
			else {
                //direct to the browser
                AuthorizeExternal();
			}

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
          
            string url =  "https://dev.pocketfulofquarters.com/access-token?app_id=" + QuartersInit.Instance.APP_ID + "&app_key=" + QuartersInit.Instance.APP_KEY;
            Application.OpenURL(url);

        }



		private void AuthorizeExternal() {

            string url = "https://dev.pocketfulofquarters.com/oauth/authorize?response_type=code&client_id=" + QuartersInit.Instance.APP_ID + "&redirect_uri=" + URL_SCHEME + "&inline=true";
			Application.OpenURL(url);

		}







		public void AuthorizationCodeReceived(string code) {

			Debug.Log("Quarters: Authorization code: " + code);
			StartCoroutine(GetRefreshToken(code));
		
		}


        //used only in Editor
        public void RefreshTokenReceived(string token) {

            Debug.Log("Quarters: Refresh token: " + token);
            RefreshToken = token;

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


            WWW www = new WWW(API_URL + "oauth/token", dataBytes, AuthorizationHeader);
			Debug.Log(www.url);

			while (!www.isDone) yield return new WaitForEndOfFrame();

			if (!string.IsNullOrEmpty(www.error)) {
				Debug.LogError(www.error);

				OnAuthorizationFailed(www.error);
			}
			else {
				Debug.Log(www.text);

				Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(www.text);
				RefreshToken = responseData["refresh_token"];
				AccessToken = responseData["access_token"];

				OnAuthorizationSuccess();
			}
		}




        public IEnumerator GetAccessToken(Action OnSuccess, Action<string> OnFailed) {

            if (string.IsNullOrEmpty(RefreshToken)) {
                Debug.LogError("Missing refresh token");
                yield break;
            }
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("grant_type", "refresh_token");
            data.Add("refresh_token", RefreshToken);
            data.Add("client_id", QuartersInit.Instance.APP_ID);
            data.Add("client_secret", QuartersInit.Instance.APP_KEY);

            string dataJson = JsonConvert.SerializeObject(data);
            Debug.Log(dataJson);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(API_URL + "oauth/token", dataBytes, AuthorizationHeader);
            Debug.Log(www.url);

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
               
                string error = www.error;
                Debug.LogError(error);

                if (error == Error.Unauthorized) {
                    //dispose invalid refresh token
                    RefreshToken = "";
                }

                OnFailed(www.error);

                
            }
            else {
                Debug.Log(www.text);

                Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(www.text);
                //RefreshToken = responseData["refresh_token"];
                AccessToken = responseData["access_token"];

                OnSuccess();
                
            }
        }



        private IEnumerator GetUserDetailsCall(OnUserDetailsSucessDelegate OnSucess, OnUserDetailsFailedDelegate OnFailed, bool isRetry = false) {

            Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
            headers.Add("Authorization", "Bearer " + AccessToken);

            WWW www = new WWW(API_URL + "me", null, headers);
			yield return www;

			while (!www.isDone) yield return new WaitForEndOfFrame();

			if (!string.IsNullOrEmpty(www.error)) {
				Debug.LogError(www.error);

                if (!isRetry) {
                    Debug.Log("Retrying");
                    //refresh access code and retry this call in case access code expired
                    StartCoroutine(GetAccessToken(delegate {
                       
                        StartCoroutine(GetUserDetailsCall(OnSucess, OnFailed, true));

                    }, delegate (string error) {
                        OnFailed(www.error);
                    }));
                } 
                else {
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
            headers.Add("Authorization", "Bearer " + AccessToken);

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

            WWW www = new WWW(API_URL + "accounts", null, headers);
            yield return www;

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
                Debug.LogError(www.error);

                if (!isRetry) {
                    Debug.Log("Retrying");
                    //refresh access code and retry this call in case access code expired
                    StartCoroutine(GetAccessToken(delegate {

                        StartCoroutine(GetAccountsCall(OnSucess, OnFailed, true));

                    }, delegate (string error) {
                        OnFailed(www.error);
                    }));
                } 
                else {
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

            if (CurrentUser == null || CurrentUser.accounts == null) {
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

            string url = API_URL + "accounts/" + account.address + "/balance";

            WWW www = new WWW(url, null, AuthorizationHeader);
            yield return www;

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
                Debug.LogError(www.error);

                if (!isRetry) {
                    Debug.Log("Retrying");
                    //refresh access code and retry this call in case access code expired
                    StartCoroutine(GetAccessToken(delegate {

                        StartCoroutine(GetAccountBalanceCall(OnSucess, OnFailed, true));

                    }, delegate (string error) {
                        OnFailed(www.error);
                    }));
                } 
                else {
                    OnFailed(www.error);
                }
            }
            else {

                Debug.Log(www.text);
                account.balance = JsonConvert.DeserializeObject<User.Account.Balance>(www.text);
                OnSucess(account.balance);

            }

        }




        private IEnumerator CreateTransferRequestCall(TransferAPIRequest request) {

            Debug.Log("CreateTransferRequestCall");
            
            Dictionary<string, string> headers = new Dictionary<string, string>(AuthorizationHeader);
            headers.Add("Authorization", "Bearer " + AccessToken);


            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("tokens", request.tokens);
            if (!string.IsNullOrEmpty(request.description)) data.Add("description", request.description);
            data.Add("app_id", QuartersInit.Instance.APP_ID);


            string dataJson = JsonConvert.SerializeObject(data);
            Debug.Log(dataJson);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(API_URL + "requests", dataBytes, headers);
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
                Application.OpenURL(url);


                //OnSucess(transferRequest);
            }
        }




        #endregion


        #region Deep linking


        void OnApplicationFocus( bool focusStatus ){
            if (focusStatus) {
                #if UNITY_ANDROID
                ProcessDeepLink();
                #endif
            }
        }




		#if UNITY_IOS

		public void DeepLink (string url) {

			Debug.Log("iOS deep link url: " + url);
            ProcessDeepLink(url);
		}



		#endif

        private void ProcessDeepLink(string url = "") {

            string linkUrl = url;

            #if UNITY_ANDROID
           
            linkUrl = CustomUrlSchemeAndroid.GetLaunchedUrl(true);
            CustomUrlSchemeAndroid.ClearSavedData();

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