using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QuartersSDK {
    public class Session {
        


        public string RefreshToken {
            get {
                return PlayerPrefs.GetString(Constants.REFRESH_TOKEN_KEY, "");
            }
            set {
                Debug.Log("Storing refresh token: " + value);
                PlayerPrefs.SetString(Constants.REFRESH_TOKEN_KEY, value);
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


        public bool DoesHaveRefreshToken {
            get {
                return !string.IsNullOrEmpty(RefreshToken);
            }
        }

        public bool DoesHaveAccessToken {
            get {
                return !string.IsNullOrEmpty(accessToken);
            }
        }


        public bool IsAuthorized {
            get {
                return DoesHaveRefreshToken;
            }
        }

        public List<Scope> Scopes = new List<Scope>();


        public void SetScope(string scopesString) {

            string scopes = UnityWebRequest.UnEscapeURL(scopesString);
            
            Scopes = new List<Scope>();

            string[] split = scopes.Split(' ');

            foreach (string scope in split) {
                Scopes.Add((Scope)Enum.Parse(typeof(Scope), scope));
            }
        }

        public bool HasScopeFor(string scope) {
            return Scopes.Contains((Scope)Enum.Parse(typeof(Scope), scope));
        }


        public void InvalidateGuestSession() {
            PlayerPrefs.DeleteKey(Constants.GUEST_TOKEN_KEY);
            PlayerPrefs.DeleteKey(Constants.GUEST_FIREBASE_TOKEN);
        }


        public static void Invalidate() {
            PlayerPrefs.DeleteKey(Constants.REFRESH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(Constants.GUEST_TOKEN_KEY);
            PlayerPrefs.DeleteKey(Constants.GUEST_FIREBASE_TOKEN);
        }


    }
}
