using QuartersSDK.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace QuartersSDK.Services
{
    public class Session
    {
        public string AccessToken {get; set;}
        public List<Scope> Scopes = new List<Scope>();
        public string RefreshToken { get; set; }

        //With Unity
        //public string RefreshToken
        //{
        //    get
        //    {
        //        return PlayerPrefs.GetString(Constants.REFRESH_TOKEN_KEY, "");
        //        return Constants.REFRESH_TOKEN_KEY;
        //    }
        //    set
        //    {
        //        Debug.Log("Storing refresh token: " + value);
        //        PlayerPrefs.SetString(Constants.REFRESH_TOKEN_KEY, value);
        //    }
        //}

        public void DoRefresh(string response)
        {
            Dictionary<string, string> responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            RefreshToken = responseData["refresh_token"];
            AccessToken = responseData["access_token"];
            SetScope(responseData["scope"]);
        }

        public void DoRefresh(ResponseData response)
        {
            RefreshToken = response.RefreshToken;
            AccessToken = response.AccessToken;
            SetScope(response.Scope);
        }

        public bool DoesHaveRefreshToken
        { 
            get { return !string.IsNullOrEmpty(RefreshToken); } 
        }

        public bool DoesHaveAccessToken
        {
            get
            {
                return !string.IsNullOrEmpty(AccessToken);
            }
        }

        public bool IsAuthorized
        {
            get
            {
                return DoesHaveRefreshToken;
            }
        }

        public void SetScope(string scopesString)
        {
            //With Unity
            //string scopes = UnityWebRequest.UnEscapeURL(scopesString);
            //scopes.Split(' ').ToList().ForEach(x => Scopes.Add((Scope)Enum.Parse(typeof(Scope), x)));

            ///Without
            scopesString.Split(' ').ToList().ForEach(x => Scopes.Add((Scope)Enum.Parse(typeof(Scope), x)));
        }

        public bool HasScopeFor(string scope)
        {
            return Scopes.Contains((Scope)Enum.Parse(typeof(Scope), scope));
        }


        public void InvalidateGuestSession()
        {
            //PlayerPrefs.DeleteKey(Constants.GUEST_TOKEN_KEY);
            //PlayerPrefs.DeleteKey(Constants.GUEST_FIREBASE_TOKEN);
        }

        public void Invalidate()
        {
            //PlayerPrefs.DeleteKey(Constants.REFRESH_TOKEN_KEY);
            //PlayerPrefs.DeleteKey(Constants.GUEST_TOKEN_KEY);
            //PlayerPrefs.DeleteKey(Constants.GUEST_FIREBASE_TOKEN);
        }
    }
}

