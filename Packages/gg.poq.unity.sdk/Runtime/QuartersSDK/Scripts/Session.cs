using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QuartersSDK {
    public class Session {
        public List<Scope> Scopes = new List<Scope>();
   
        public string RefreshToken {
            get => PlayerPrefs.GetString(Constants.REFRESH_TOKEN_KEY, "");
            set {
                PlayerPrefs.SetString(Constants.REFRESH_TOKEN_KEY, value);
            }
        }
   
        public string AccessToken { get; set; } = "";
   
   
        public bool DoesHaveRefreshToken => !string.IsNullOrEmpty(RefreshToken);
   
        public bool DoesHaveAccessToken => !string.IsNullOrEmpty(AccessToken);
   
   
        public bool IsAuthorized => DoesHaveRefreshToken;
   
        public void SetScope(string scopesString) {
            Scopes = new List<Scope>();
            
            string scopes = UnityWebRequest.UnEscapeURL(scopesString);
            string[] split = scopes.Split(' ');
            
            foreach (string scope in split) Scopes.Add((Scope) Enum.Parse(typeof(Scope), scope));
        }
   
        public bool HasScopeFor(string scope) {
            return Scopes.Contains((Scope) Enum.Parse(typeof(Scope), scope));
        }
        public void SignOut()
        {
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
            Invalidate();
        }

        public static void Invalidate() {
            PlayerPrefs.DeleteKey(Constants.REFRESH_TOKEN_KEY);
        }
    }
}