using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;


namespace Quarters {
	public class Quarters {

		public static Quarters Instance;

		public delegate void OnAuthorizationStartDelegate();
		public static event OnAuthorizationStartDelegate OnAuthorizationStart;


	
		public const string QUARTERS_URL = "https://pocketfulofquarters.com";
		public const string API_URL = "https://api.dev.pocketfulofquarters.com/v1/";

		public string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJ4WEJTT2RNb0FIZGxDNTI1QXpXc1RZQzRyRnQyIiwicmFuZG9tIjoiNmJhMmU2MzAtMTE5NC0xMWU4LTk0YjgtOTFhMmJhN2M1OTcyIiwiYXBwSWQiOiJqUjNHaktIVVV0Um9JNmU4RFBzMiIsInRva2VuVHlwZSI6Imp3dDphdXRob3JpemF0aW9uX2NvZGUiLCJpYXQiOjE1MTg2MTg5NzksImV4cCI6MTUxODYxOTAzOX0.3tVezdZxweIVY0XdclGsLI5_pAblqhuwSeBFxbpVFPk";


		public Quarters() {
			Instance = this;
		}




		public void Authorize() {

			Debug.Log("Quarters: Authorize");

			if (OnAuthorizationStart != null) OnAuthorizationStart();

			if (Application.isEditor) {
				GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("QuartersEditor"));
			}
			else {
				Debug.LogError("Missing implementation here");
			}
				
		}



		public void AuthorizationCodeReceived(string code) {

			Debug.Log("Quarters: Authorization code: " + code);
		}




		public IEnumerator GetUserDetails() {

			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("Authorization", "Bearer " + accessToken);

			WWW www = new WWW(API_URL + "me", null, headers);
			yield return www;
			Debug.Log(www.error);


			while (!www.isDone) yield return new WaitForEndOfFrame();

			if (!string.IsNullOrEmpty(www.error)) {
				Debug.LogError(www.error);
			}
			else {

				Debug.Log(www.text);
				
			}

		}




		public IEnumerator GetRefreshToken(string code) {

			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json;charset=UTF-8");


			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("grant_type", "authorization_code");
			data.Add("code", code);
			data.Add("client_id", QuartersInit.APP_ID);
			data.Add("client_secret", QuartersInit.APP_KEY);

			string dataJson = JsonConvert.SerializeObject(data);
			Debug.Log(dataJson);
			byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


			WWW www = new WWW(API_URL + "oauth/token", dataBytes, headers);
			Debug.Log(www.url);

			while (!www.isDone) yield return new WaitForEndOfFrame();

			if (!string.IsNullOrEmpty(www.error)) {
				Debug.LogError(www.error);
			}
			else {

				Debug.Log(www.text);

			}



		}




	}
}