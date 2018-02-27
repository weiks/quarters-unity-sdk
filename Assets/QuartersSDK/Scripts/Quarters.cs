using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;


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
		public OnUserDetailsSucessDelegate OnUserDetailsSucess;

		public delegate void OnUserDetailsFailedDelegate(string error);
		public OnUserDetailsFailedDelegate OnUserDetailsFailed;


		public const string QUARTERS_URL = "https://pocketfulofquarters.com";
		public const string API_URL = "https://api.dev.pocketfulofquarters.com/v1/";

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
			this.OnUserDetailsSucess = OnSuccessDelegate;
			this.OnUserDetailsFailed = OnFailedDelegate;

			StartCoroutine(GetUserDetails());
		}

        #endregion




        private void AuthorizeEditor() {
          
            string url =  "https://dev.pocketfulofquarters.com/access-token?app_id=" + QuartersInit.Instance.APP_ID + "&app_key=" + QuartersInit.Instance.APP_KEY;
            Application.OpenURL(url);

        }



		private void AuthorizeExternal() {

            //temporary redirect
            string redirectUrl = "quarters://";

            string url = "https://dev.pocketfulofquarters.com/oauth/authorize?response_type=code&client_id=" + QuartersInit.Instance.APP_ID + "&redirect_uri=" + redirectUrl + "&inline=true";
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

			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json;charset=UTF-8");


			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("grant_type", "authorization_code");
			data.Add("code", code);
            data.Add("client_id", QuartersInit.Instance.APP_ID);
            data.Add("client_secret", QuartersInit.Instance.APP_KEY);

			string dataJson = JsonConvert.SerializeObject(data);
			Debug.Log(dataJson);
			byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


			WWW www = new WWW(API_URL + "oauth/token", dataBytes, headers);
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


            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json;charset=UTF-8");


            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("grant_type", "refresh_token");
            data.Add("refresh_token", RefreshToken);
            data.Add("client_id", QuartersInit.Instance.APP_ID);
            data.Add("client_secret", QuartersInit.Instance.APP_KEY);

            string dataJson = JsonConvert.SerializeObject(data);
            Debug.Log(dataJson);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(API_URL + "oauth/token", dataBytes, headers);
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





        private IEnumerator GetUserDetails(bool isRetry = false) {

            //AccessToken = "a";

			Dictionary<string, string> headers = new Dictionary<string, string>();
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

                        StartCoroutine(GetUserDetails(true));

                    }, delegate (string error) {

                        OnUserDetailsFailed(www.error);

                    }));
                } 
                else {
                    OnUserDetailsFailed(www.error);
                }
			}
			else {


				Debug.Log(www.text);
				CurrentUser = JsonConvert.DeserializeObject<User>(www.text);
				OnUserDetailsSucess(CurrentUser);
				
			}

		}



        #endregion




	}
}