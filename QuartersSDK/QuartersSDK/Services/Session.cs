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
        public string RefreshToken { get; set; }
        public string AccessToken {get; set;}
        public List<Scope> Scopes = new List<Scope>();

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
            scopesString.Split(' ').ToList().ForEach(x => Scopes.Add((Scope)Enum.Parse(typeof(Scope), x)));
        }

        public bool HasScopeFor(string scope)
        {
            return Scopes.Contains((Scope)Enum.Parse(typeof(Scope), scope));
        }

        public void Invalidate()
        {
            this.AccessToken = "";
            this.RefreshToken = "";
        }
    }
}

